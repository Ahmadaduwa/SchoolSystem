using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.ActivityManagement
{
    public class ActivityAttendance
    {
        [Key]
        public int AA_id { get; set; }
        [Required]

        public int AM_id { get; set; }
        [ForeignKey("AM_id")]
        public virtual ActivityManagement? ActivityManagement { get; set; }

        [Required]
        public int Student_id { get; set; }
        [ForeignKey("Student_id")]
        public virtual Student? Student { get; set; }

        [Required]
        [StringLength(20)]
        public String? Status { get; set; }

        public DateOnly TimeStamp { get; set; }
    }
}
