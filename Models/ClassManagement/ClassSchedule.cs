using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{

    public class ClassSchedule
    {
        [Key]
        public int ScheduleID { get; set; } // Primary Key

        
        public int CM_ID { get; set; } // Foreign Key to ClassManagement
        public ClassManagement? ClassManagement { get; set; } // Navigation Property

        [Required]
        public string DayOfWeek { get; set; } = string.Empty; // e.g., Monday, Tuesday

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; } // Start Time

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; } // End Time

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Status (e.g., Active/Inactive)
    }

}
