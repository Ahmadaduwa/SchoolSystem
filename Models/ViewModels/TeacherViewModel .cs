using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.ViewModels
{
    public class TeacherViewModel
    {
        // ✅ Profile Information
        public int? ProfileId { get; set; } // ✅ เช็คให้แน่ใจว่ามี ProfileId
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        public string Gender { get; set; } = "Not Specified";
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        // ✅ Teacher Information
        public int? DepartmentId { get; set; } // ✅ เปลี่ยนให้เป็น nullable
        [Required]
        public DateTime HireDate { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal Salary { get; set; }
        public string Status { get; set; } = "Active";

        private List<SelectListItem> _departments = new List<SelectListItem>();
        public List<SelectListItem> Departments
        {
            get => _departments;
            set => _departments = value;
        }
    }
}
