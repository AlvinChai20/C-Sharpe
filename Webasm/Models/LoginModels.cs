using System.ComponentModel.DataAnnotations;

namespace UsersApp.ViewModels
{
    public class LoginModels
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public bool ShowCaptcha { get; set; } = true;  // default true for now

        public string CaptchaToken { get; set; }

    }
}