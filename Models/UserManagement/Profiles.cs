using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SchoolSystem.Models.Alert;
using SchoolSystem.Models.RegistrationManagement;

namespace SchoolSystem.Models.UserManagement
{
    public class Profiles
    {
        [Key]
        public int ProfileId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Gender { get; set; } = "Not Specified";

        public string? Address { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public Users? User { get; set; }
        public Teacher? Teacher { get; set; }
        public Student? Student { get; set; }
        public virtual ICollection<Notification> Notification { get; set; } = new List<Notification>();
    }
}
