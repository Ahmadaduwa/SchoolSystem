using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class SubjectCategory
    {
        [Key]
        public int SubjectCategoryId { get; set; } // Primary Key

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty; // Category Name

        public string? Description { get; set; } // Optional Description

        // Relationships
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
