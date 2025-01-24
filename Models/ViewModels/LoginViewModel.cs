using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 30 characters.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
