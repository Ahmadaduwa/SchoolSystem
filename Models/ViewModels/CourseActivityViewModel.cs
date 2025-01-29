using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Models.ViewModels
{
    public class CourseActivityViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Course_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        public List<Activity> Activities { get; set; } = new List<Activity>();
    }
}
