using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    [Authorize] // only logged-in users can access
    public class ProfileController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProfileController(UserManager<Users> userManager, AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        // ------------------- View Profile -------------------
        public async Task<IActionResult> ViewProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null) return NotFound();

            var model = new ProfileModels
            {
                Name = profile.FullName,
                Email = user.Email,
                ProfilePictureUrl = profile.ProfilePictureUrl
            };

            return View(model);
        }

        // ------------------- Edit Profile -------------------
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null) return NotFound();

            var model = new ProfileModels
            {
                Name = profile.FullName,
                Email = user.Email,
                ProfilePictureUrl = profile.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileModels model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null) return NotFound();

            // ✅ Update name
            profile.FullName = model.Name;

            // ✅ Update profile picture if uploaded
            if (model.ProfilePhoto != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePhoto.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePhoto.CopyToAsync(fileStream);
                }

                profile.ProfilePictureUrl = "/images/profile/" + uniqueFileName;
                user.ProfilePictureUrl = profile.ProfilePictureUrl; // keep in sync
            }

            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();

            await userManager.UpdateAsync(user);

            return RedirectToAction("ViewProfile");
        }
    }
}
