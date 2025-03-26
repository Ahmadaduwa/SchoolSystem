using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.ViewModels
{

    public class StudentCheckViewModel
    {
        public int StudentId { get; set; } // รหัสนักเรียน

        public string? StudentName { get; set; } // ชื่อนักเรียน (ถ้าต้องการใช้แสดงใน View)

        [Required]
        public string Status { get; set; } = "Present"; // ค่าเริ่มต้นคือ "Present"

        public bool IsChecked { get; set; } // ใช้เพื่อแสดง Checkbox
    }
}
