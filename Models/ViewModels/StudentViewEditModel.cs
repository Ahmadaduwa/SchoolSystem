using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolSystem.ViewModels
{
    public class StudentViewEditModel
    {
        // ข้อมูลนักเรียนเฉพาะ
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student Code is required.")]
        [StringLength(10, ErrorMessage = "Student Code cannot exceed 10 characters.")]
        [Display(Name = "Student Code")]
        public string Student_Code { get; set; }

        [Required(ErrorMessage = "Class selection is required.")]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Enrollment Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "GPA")]
        [Range(0.00, 4.00, ErrorMessage = "GPA must be between 0.00 and 4.00.")]
        public float GPA { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active";

        // ข้อมูลส่วนตัว (Profile)
        public int ProfileId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        [RegularExpression("^(Male|Female|Other|Not Specified)$", ErrorMessage = "Invalid gender selection.")]
        public string Gender { get; set; } = "Not Specified";

        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Picture URL")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Profile Picture")]
        // [FilePicture] // Comment out or remove this line until the attribute is defined
        public IFormFile? ProfilePicture { get; set; }

        // ข้อมูลบัญชีผู้ใช้ (Account)
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, ErrorMessage = "Username cannot exceed 30 characters.")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; } = "Student";

        [Display(Name = "Class Name")]
        public string? ClassName { get; set; }

        public bool? HasStudentCouncilRole { get; set; }

        // Dropdown List สำหรับข้อมูลเพิ่มเติม
        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();
    }
}
