using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassManagement
    {

    [Key] // Mark CM_ID as the primary key
        public int CM_ID { get; set; } // Primary Key

        public int ClassID { get; set; } // Foreign Key to Classes
        public Class? Class { get; set; } // Navigation Property

        public int TeacherID { get; set; } // Foreign Key to Teachers
        public  Teacher? Teacher { get; set; } // Navigation Property

        public int SubjectID { get; set; } // Foreign Key to Subjects
        public  Subject? Subject { get; set; } // Navigation Property

        public int SemesterID { get; set; } // Foreign Key to Semesters
        public Semester? Semester { get; set; } // Navigation Property

        public int CourseID { get; set; } // Navigation Property

        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

        public string? ScoringCriteria { get; set; } // Optional scoring criteria
        public int CheckCount { get; set; } // Attendance or management count

        public DateTime UpdatedAt { get; set; } // Last modification timestamp
        

    }
}

