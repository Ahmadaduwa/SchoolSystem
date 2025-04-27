namespace SchoolSystem.Models.ViewModels
{
    public class AssignmentGradingViewModel
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int CM_Id { get; set; }
        
        public float FullScore { get; set; }

        public DateTime DueDate { get; set; }
        public List<StudentScoreViewModel> StudentScores { get; set; } = new List<StudentScoreViewModel>();
    }

    public class StudentScoreViewModel
    {
        public int StudentId { get; set; }

        public string Student_Code { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
