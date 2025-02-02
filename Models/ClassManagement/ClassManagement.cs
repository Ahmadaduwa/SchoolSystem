using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassManagement
    {
        [Key]
        public int CM_Id { get; set; }

        [Required]
        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [Required]
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required]
        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Scoring criteria must be less than 255 characters.")]
        public string ScoringCriteria { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "CheckCount must be a non-negative integer.")]
        public int CheckCount { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public virtual Class Class { get; set; } // Navigation Property
        public virtual Teacher Teacher { get; set; } // Navigation Property
        public virtual Course Course { get; set; } // Navigation Property
        public virtual Semester Semester { get; set; } // Navigation Property
    }
}
