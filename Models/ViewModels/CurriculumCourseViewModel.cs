using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.ViewModels
{
    public class CurriculumCourseViewModel
    {
        public int CurriculumId { get; set; }
        public string CurriculumName { get; set; }
        public List<CompulsoryCourse> CompulsoryCourses { get; set; } = new();
        public List<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new();
        public List<ElectiveCourse> ElectiveCourses { get; set; } = new();
    }
}
