using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassManagement
    {
        [Key]
        public int CM_Id { get; set; }

        [Required]
        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [Required]
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required]
        [ForeignKey("Semester")]
        public int SemesterId { get; set; }

        [StringLength(255, ErrorMessage = "Scoring criteria must be less than 255 characters.")]
        public string? ScoringCriteria { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "CheckCount must be a non-negative integer.")]
        public int CheckCount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public virtual Class? Class { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual Course? Course { get; set; }
        public virtual Semester? Semester { get; set; }

        public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
        public virtual ICollection<ClassAttendanceSummary> ClassAttendanceSummary { get; set; } = new List<ClassAttendanceSummary>();
        public virtual ICollection<ClassAttendance> ClassAttendance { get; set; } = new List<ClassAttendance>();
        public virtual ICollection<ClassAttendanceCheck> ClassAttendanceCheck { get; set; } = new List<ClassAttendanceCheck>();
        public virtual ICollection<Assignment.Assignment> Assignment { get; set; } = new List<Assignment.Assignment>();
    }
}
