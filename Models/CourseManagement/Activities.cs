using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Activities
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        [StringLength(100)] // กำหนดความยาวสูงสุดของ ActivityName
        public string? ActivityName { get; set; }
    }
}
