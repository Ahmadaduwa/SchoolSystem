using Microsoft.AspNetCore.Identity;

namespace SchoolSystem.Models
{
    public class Users : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }
}
