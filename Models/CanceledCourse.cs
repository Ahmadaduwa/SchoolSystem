using System.ComponentModel.DataAnnotations;
namespace SchoolSystem.Models
{
    public class CanceledCourse
    {
        [Key]
        public int CanceledCourseID { get; set; }
        public int CourseID { get; set; }
        public required string CourseCode { get; set; }
        public required string CourseName { get; set; }
        public string? LearningObjective { get; set; }
        public int Units { get; set; }
        public string? Category { get; set; }
        public string? Prerequisites { get; set; }
        public string? Postrequisites { get; set; }
        public DateTime CanceledDate { get; set; }
        public string? Reason { get; set; }
    }
}
