namespace SchoolSystem.Models.ViewModels
{
    public class StudentCheckViewModel
    {
        public int Id { get; set; } // ลำดับ
        public string? StudentId { get; set; } // รหัสนักเรียน
        public string? FirstName { get; set; } // ชื่อ
        public string? LastName { get; set; } // นามสกุล
        public bool IsChecked { get; set; } // สถานะการเช็คชื่อ
        public string Status { get; set; }
    }
}
