using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.UserManagement
{
    public class Guardian
    {
        [Key]
        public int GuardianId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public string? Relationship { get; set; }

        public string? Occupation { get; set; }

        public int? ProfileId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        
    }
}
