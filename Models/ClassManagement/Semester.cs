using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Models.ClassManagement
{
    public class Semester
    {
        [Key]
        public int SemesterID { get; set; } // Primary Key

        [Required(ErrorMessage = "Semester Year is required.")]
        [StringLength(10, ErrorMessage = "Semester Year cannot exceed 10 characters.")]
        [Display(Name = "Semester Year")]
        public string SemesterYear { get; set; } = string.Empty; // ไม่อนุญาตให้เป็น null

        [Required(ErrorMessage = "Semester Number is required.")]
        [Range(1, 3, ErrorMessage = "Semester number must be between 1 and 3.")]
        [Display(Name = "Semester Number")]
        public int SemesterNumber { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        [DateAfter("StartTime", ErrorMessage = "End date must be after the start date.")]
        [Display(Name = "End Date")]
        public DateTime EndTime { get; set; }

        // Navigation Properties
        public virtual ICollection<ClassManagement> ClassManagements { get; set; } = new HashSet<ClassManagement>();
        public virtual ICollection<ActivityManagement.ActivityManagement> ActivityManagement { get; set; } = new List<ActivityManagement.ActivityManagement>();
    }

    public class DateAfterAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateAfterAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
                return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var comparisonValue = (DateTime)comparisonProperty.GetValue(validationContext.ObjectInstance);
            var currentValue = (DateTime)value;

            // ✅ แปลงเฉพาะส่วนของ "Date" มาเปรียบเทียบกัน
            if (currentValue.Date <= comparisonValue.Date)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
