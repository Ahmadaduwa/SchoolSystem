using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.ActivityManagement
{
    public class ActivityAttendanceSummary
    {
        public int AAM_Id { get; set; }

        [Required]
        [ForeignKey("CM_Id")]
        public int AM_id { get; set; }
        public virtual ActivityManagement? ActivityManagement { get; set; }

        [Required]
        [ForeignKey("StudentId")]
        public int? StudentId { get; set; }
        public virtual Student? Student { get; set; }
        public int? PresentCount { get; set; } //มา
        public int? AbsentCount { get; set; } //ไม่มา
        public int? LateCount { get; set; } //มาสาย
        public int? ExcusedCount { get; set; } //ลา
        
        public DateTime UpdateAt { get; set; }
    }
}
