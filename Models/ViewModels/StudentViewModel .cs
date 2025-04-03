using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolSystem.ViewModels
{
    public class StudentViewModel
    {
        // ข้อมูลนักเรียน (Student)
        [Required]
        [Display(Name = "Student Code")]
        public string Student_Code { get; set; }

        [Required]
        [Display(Name = "Class Id")]
        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "GPA")]
        public float GPA { get; private set; } // ป้องกันไม่ให้แก้ไข GPA โดยตรง

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        // ข้อมูลส่วนตัว (Profile)
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; } = "Not Specified";

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        // ข้อมูลบัญชีผู้ใช้ (Account)
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        // Dropdown List สำหรับข้อมูลเพิ่มเติม
        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();

        // ฟังก์ชันอัปเดต GPA โดยไม่ให้แก้ไขจากภายนอก
        public void UpdateGPA(float newGPA)
        {
            GPA = newGPA;
        }
    }
}
