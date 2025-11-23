using Microsoft.AspNetCore.Mvc;
using UsersApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UsersApp.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 6; // Show 6 products per page

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        // BookNow main page (just loads the view, AJAX will load products)
        public IActionResult BookNow()
        {
            return View();
        }

        // Load products dynamically with AJAX (search, sort, paging)
        [HttpGet]
        public IActionResult LoadProducts(int page = 1, string? search = "", string? sort = "")
        {
            var query = _context.Products.AsQueryable();

            // 🔎 Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // ↕ Sort
            query = sort switch
            {
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Id)
            };

            // 📄 Paging
            int totalItems = query.Count();
            var products = query.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // Render partial view
            string productsHtml = RenderPartialViewToString(this, "_ProductCards", products);
            string paginationHtml = RenderPagination(page, totalItems, PageSize);

            return Json(new { productsHtml, paginationHtml });
        }

        // 📌 Product details page
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // calculate deposit (20%)
            ViewBag.Deposit = Math.Round(product.Price * 0.2m, 2);

            return View(product); // Views/Book/Details.cshtml
        }

        // ✅ Helper to render partials
        private static string RenderPartialViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using var sw = new StringWriter();
            var viewEngine = controller.HttpContext.RequestServices
                .GetService(typeof(Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine))
                as Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine;

            var view = viewEngine.FindView(controller.ControllerContext, viewName, false).View;
            var viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData,
                controller.TempData, sw, new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions());

            view.RenderAsync(viewContext).Wait();
            return sw.GetStringBuilder().ToString();
        }

        private static string RenderPagination(int currentPage, int totalItems, int pageSize)
        {
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages <= 1) return "";

            var sb = new System.Text.StringBuilder();
            for (int i = 1; i <= totalPages; i++)
            {
                sb.Append($"<li class='page-item {(i == currentPage ? "active" : "")}'>" +
                          $"<a class='page-link' href='#' data-page='{i}'>{i}</a></li>");
            }
            return sb.ToString();
        }
    }
}
