using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SchoolSystem.Models
{
    public class Profiles
    {
        [Key]
        public int ProfileId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Gender { get; set; } = "Not Specified";

        public string Address { get; set; } = string.Empty;

        private DateOnly _dateOfBirth;
        public DateOnly DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (value == default)
                    throw new ArgumentException("Date of Birth cannot be the default value.");
                _dateOfBirth = value;
            }
        }
        [Url] // Data Annotation to validate URL format
        public string ProfilePictureUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }
    }
}
