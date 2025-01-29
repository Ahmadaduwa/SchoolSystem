using SchoolSystem.Models.CourseManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

        public List<Activities> Activities { get; set; }  // Added a collection for Activities

        // Constructor to initialize the Activities list
        public CourseActivityViewModel()
        {
            Activities = new List<Activities>();
        }
    }
}
