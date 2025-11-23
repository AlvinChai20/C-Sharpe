using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using UsersApp.Models;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly AppDbContext _context;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, IWebHostEnvironment webHostEnvironment, AppDbContext context) // ✅ inject DbContext here)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            _context = context;

        }

        // ---------------- LOGIN ----------------
        public IActionResult Login()
        {
            var model = new LoginModels
            {
                ShowCaptcha = true
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModels model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔹 Step 1: Verify reCAPTCHA
            var secretKey = "6Lfa0M4rAAAAAO8aaZeSFZvkujTNv_51fBKzQ7f_";
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={model.CaptchaToken}",
                null);

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            if (result.success != true)
            {
                ModelState.AddModelError(string.Empty, "Captcha validation failed.");
                return View(model);
            }

            // 🔹 Step 2: User existence check
            var user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            // 🔹 Step 3: Lockout check
            if (await userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("", "Your account has been locked out due to multiple failed login attempts. Please try again after 5 minutes.");
                return View(model);
            }

            // 🔹 Step 4: Try signing in
            var resultSignIn = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (resultSignIn.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);

                if (await userManager.IsInRoleAsync(user, "Staff"))
                {
                    return RedirectToAction("StaffPage", "Home");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (resultSignIn.IsLockedOut)
                {
                    ModelState.AddModelError("", "Your account has been locked out due to 3 failed login attempts. Please try again after 5 minutes.");
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect.");
                }
                return View(model);
            }
        }

        // ---------------- REGISTER ----------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModels model, IFormFile ProfilePhoto)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ Step 1: Google reCAPTCHA validation
            var captchaResponse = Request.Form["g-recaptcha-response"];
            var secretKey = "6Lfa0M4rAAAAAO8aaZeSFZvkujTNv_51fBKzQ7f_";

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}",
                null);

            var json = await response.Content.ReadAsStringAsync();
            dynamic captchaResult = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            if (captchaResult.success != true)
            {
                ModelState.AddModelError(string.Empty, "Captcha validation failed.");
                return View(model);
            }

            // ✅ Step 2: Handle profile photo upload
            string profilePictureUrl = "/images/default-profile.png"; // fallback default

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(fileStream);
                }

                profilePictureUrl = "/images/profile/" + uniqueFileName;
            }

            // ✅ Step 3: Create Identity user
            var user = new Users
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,
                ProfilePictureUrl = profilePictureUrl
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // ✅ Step 4: Assign role
                await userManager.AddToRoleAsync(user, model.Role);

               

                // ✅ Step 6: Sign in user after registration
                await signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // ✅ Step 7: Handle Identity errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        // ---------------- PROFILE ----------------
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new ProfileModels
            {
                Name = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileModels model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = $"Unable to load user with ID '{userManager.GetUserId(User)}'." });
            }

            bool changesMade = false;

            if (user.FullName != model.Name)
            {
                user.FullName = model.Name;
                await userManager.UpdateAsync(user);
                changesMade = true;
            }

            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    var errors = changePasswordResult.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, message = string.Join(" ", errors) });
                }
                changesMade = true;
            }

            string newProfilePictureUrl = user.ProfilePictureUrl;
            if (model.ProfilePhoto != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePhoto.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePhoto.CopyToAsync(fileStream);
                }

                newProfilePictureUrl = "/images/profile/" + uniqueFileName;
                user.ProfilePictureUrl = newProfilePictureUrl;
                await userManager.UpdateAsync(user);
                changesMade = true;
            }

            if (changesMade)
            {
                return Json(new { success = true, message = "Profile updated successfully!", profilePictureUrl = newProfilePictureUrl });
            }
            else
            {
                return Json(new { success = true, message = "No changes were made to the profile." });
            }
        }

        // ---------------- PASSWORD RESET ----------------
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModels model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }

                // Generate a secure reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                // Build a link to your ChangePassword action
                var resetLink = Url.Action("ChangePassword", "Account",
                    values: new { email = user.Email, token = token }, Request.Scheme);

                // Send the email
                await SendVerificationEmail(user.Email, resetLink);

                ViewData["SuccessMessage"] = "We have sent a password reset link to your email. Please check your inbox.";
                return View();
            }
            return View(model);
        }
        private async Task SendVerificationEmail(string email, string link)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.Credentials = new NetworkCredential("yangzuer74@gmail.com", "xshw zkzh zwop vlor");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("yangzuer74@gmail.com", "DentQ"),
                    Subject = "Password Reset Request",
                    Body = $"Click the following link to reset your password: <a href='{link}'>Reset Password</a>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }

        public IActionResult ChangePassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("VerifyEmail");
            }

            return View(new ChangePasswordModels { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModels model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }

                // Reset password using the token
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // ---------------- LOGOUT ----------------
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------------- DELETE OWN ACCOUNT ----------------
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> DeleteOwnAccount()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            await signInManager.SignOutAsync();
            await userManager.DeleteAsync(user);

            return RedirectToAction("Index", "Home");
        }
    }
}
