using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Models.ViewModels
{
    public class CurriculumActivityViewModel
    {
        [Required]
        public int CurriculumId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Curriculum_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CurriculumName { get; set; } = string.Empty;

        public List<Activity> Activities { get; set; } = new List<Activity>();
    }
}
