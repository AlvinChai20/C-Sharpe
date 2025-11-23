using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;

using System.Security.Claims;

namespace UsersApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // Simple fixed price mapping
        private readonly Dictionary<string, decimal> _servicePrices = new()
    {
        { "Dental Scaling", 150 },
        { "Dental Filling", 200 },
        { "Braces", 5000 },
        { "Teeth Whitening", 400 }
    };

        // GET: /Payment/Index/5
        public async Task<IActionResult> Index(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.AspNetAppointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found or not yours.";
                return RedirectToAction("List", "Booking");
            }

            // Lookup price without storing in DB
            var price = _servicePrices.ContainsKey(appointment.Service)
                        ? _servicePrices[appointment.Service]
                        : 0;

            ViewBag.Price = price;
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int appointmentId, string paymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointment = await _context.AspNetAppointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found or not yours.";
                return RedirectToAction("List", "Booking");
            }

            appointment.PurchaseStatus = "Successful";
            appointment.PaidBy = paymentMethod;   // ✅ Save how user paid

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment successful!";
            return RedirectToAction("List", "Booking");
        }

    }
}

