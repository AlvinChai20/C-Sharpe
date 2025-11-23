using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UsersApp.ViewModels
{
    public class ProfileModels
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public string Email { get; set; }

        public string ProfilePictureUrl { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("ConfirmNewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile ProfilePhoto { get; set; }
    }
}
