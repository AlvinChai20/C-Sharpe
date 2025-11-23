using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsersApp.Models
{
    public class UserProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string Role { get; set; }

        public string ProfilePictureUrl { get; set; } = "/images/default.png"; // default pic

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
