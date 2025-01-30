using SchoolSystem.Models.CurriculumManagement;
using System.Collections.Generic;

namespace SchoolSystem.Models.ViewModels
{
    public class ManageCoursesViewModel
    {
        public int CurriculumId { get; set; }
        public string? CurriculumName { get; set; }
        public List<ElectiveCourse>? ElectiveCourses { get; set; }
        public List<CompulsoryCourse>? CompulsoryCourses { get; set; }
        public List<CompulsoryElectiveCourse>? CompulsoryElectiveCourses { get; set; }
    }
}
