using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ClassManagement
{
    public class Semester
    {
        [Key]
        public int SemesterId { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

    }
}
