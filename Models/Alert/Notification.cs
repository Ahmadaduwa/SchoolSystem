using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.Alert
{// use this model
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(50)]
        public string? NotificationType { get; set; }

        [Required(ErrorMessage = "Please provide the date and time of the notification")]
        [DataType(DataType.DateTime)]
        public DateTime NotificationTime { get; set; }

        [Required(ErrorMessage = "Please enter the notification message")]
        public string? Message { get; set; }

        [Required]
        public int ProfileId { get; set; }
        [ForeignKey("ProfileId")]
        public virtual Profiles? Profile { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } // e.g., "Success", "Failed", "Pending"

    }
}
