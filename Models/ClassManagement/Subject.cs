using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class Subject
    {
        [Key]
        public int SubjectID { get; set; } // Primary Key

        [Required]
        [MaxLength(50)]
        public required string SubjectCode { get; set; } // Subject Code

        [Required]
        [MaxLength(200)]
        public required string SubjectName { get; set; } // Subject Name

        public string? Description { get; set; } // Optional Description

        [Required]
        [StringLength(20)]
        public required string Status { get; set; } // Status (e.g., Active/Inactive)

        public int SubjectCategoryId { get; set; } // Foreign Key to SubjectCategory
        public virtual SubjectCategory? SubjectCategory { get; set; } // Navigation Property

        // Relationships
        public virtual ICollection<ClassManagement> ClassManagements { get; set; } = new List<ClassManagement>();
    }

}
