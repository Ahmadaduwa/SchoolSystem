using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ViewModels;

namespace SchoolSystem.ViewModels
{
    public class AttendanceViewModel
    {
        public int cmId { get; set; }
        [DataType(DataType.Date)]
        public DateOnly date { get; set; }
        public List<StudentAttendanceInputModel> Students { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
    }
}