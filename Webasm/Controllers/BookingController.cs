using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using UserApp.Models;
using UsersApp.Models;
using Microsoft.AspNetCore.Identity;

public class BookingController : Controller
{
    private readonly UserManager<Users> _userManager;
    private readonly AppDbContext _context;

    public BookingController(AppDbContext context, UserManager<Users> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ================= STAFF APPOINTMENT HISTORY =================
    [Authorize(Roles = "Staff")]
    public IActionResult AppointmentHistory()
    {
        ViewBag.Users = _context.Users
            .Select(u => new { u.Id, u.UserName })
            .ToList();

        return View();
    }

    [Authorize(Roles = "Staff")]
    public IActionResult LoadAppointments(string search, string userId, int page = 1, int pageSize = 5)
    {
        var query = _context.AspNetAppointments.AsQueryable();

        // 🔍 Search
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a =>
                a.ClientName.Contains(search) ||
                a.Service.Contains(search) ||
                a.ContactNumber.Contains(search));
        }

        // 🎯 Filter by user
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        var total = query.Count();

        var data = query
            .OrderByDescending(a => a.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new {
                a.ClientName,
                a.ContactNumber,
                a.Service,
                a.Date,
                a.Status,
                a.PurchaseStatus
            })
            .ToList();

        return Json(new { data, total });
    }


    // ================= USER BOOKING =================
    [Authorize]
    public IActionResult List()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var appointments = _context.AspNetAppointments
            .Where(a => a.UserId == userId)
            .ToList();

        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string service, string price, string deposit)
    {
        var user = await _userManager.GetUserAsync(User);

        ViewBag.Service = service;
        ViewBag.Price = price;
        ViewBag.Deposit = deposit;
        ViewBag.UserName = user?.FullName ?? user?.UserName; // ✅ send logged in user's name

        return View();
    }


    [HttpPost]
    public IActionResult Proceed(Appointments appointment, string Price)
    {
        appointment.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _context.AspNetAppointments.Add(appointment);
        _context.SaveChanges();

        return RedirectToAction("Index", "Payment", new { appointmentId = appointment.Id, price = Price });
    }

    [HttpPost]
    [Authorize]
    public IActionResult Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var appointment = _context.AspNetAppointments.FirstOrDefault(a => a.Id == id && a.UserId == userId);

        if (appointment == null)
        {
            return NotFound();
        }

        _context.AspNetAppointments.Remove(appointment);
        _context.SaveChanges();

        TempData["Message"] = "Booking deleted successfully.";
        return RedirectToAction("List");
    }

    public IActionResult Edit(int id)
    {
        var appointment = _context.AspNetAppointments.Find(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Appointments appointment)
    {
        if (id != appointment.Id) return NotFound();

        var existing = _context.AspNetAppointments.FirstOrDefault(a => a.Id == id);
        if (existing == null) return NotFound();

        // ✅ Update only editable fields
        existing.ClientName = appointment.ClientName;
        existing.ContactNumber = appointment.ContactNumber;
        existing.Service = appointment.Service;
        existing.Date = appointment.Date;

        if (ModelState.IsValid)
        {
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Booking updated successfully!";
            return RedirectToAction(nameof(List));
        }

        return View(appointment);
    }

    [HttpGet]
    public async Task<IActionResult> BookNow(string service, decimal price)
    {
        var user = await _userManager.GetUserAsync(User);

        ViewBag.Service = service;
        ViewBag.Price = price;
        ViewBag.UserName = user?.FullName ?? user?.UserName;

        return View();
    }
}
