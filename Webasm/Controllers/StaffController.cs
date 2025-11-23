using Microsoft.AspNetCore.Mvc;
using UsersApp.Models;

namespace UsersApp.Controllers
{
    public class StaffController : Controller
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult StaffAddProduct()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StaffAddProduct(Product product)
        {
            // ✅ extra server-side defensive check
            if (product.Price <= 0)
            {
                ModelState.AddModelError(nameof(product.Price), "Price must be a positive value.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["Success"] = "Product added successfully.";

            // redirect to BookNow after save
            return RedirectToAction("BookNow", "Book");
        }
    }
}
