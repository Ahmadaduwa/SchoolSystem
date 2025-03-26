using SchoolSystem.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ViewModels
{
    public class ActivityAttendanceViewModel
    {
        public int ActivityManagementId { get; set; }
        [Required]
        public string ActivityName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int ClassId { get; set; }
        public List<StudentAttendanceInputModel> Students { get; set; }
    }
}
