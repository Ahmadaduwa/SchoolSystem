using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ViewModels
{
    public class AssignmentSummaryViewModel
    {
        public int CM_Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<StudentSummaryViewModel> StudentSummaries { get; set; } = new();
    }

    public class StudentSummaryViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public float TotalScore { get; set; }
        public float Grade { get; set; }
        public bool IsRegistered { get; set; }
    }
}