using Microsoft.AspNetCore.Identity;
using System;

namespace SchoolSystem.Models
{
    public class Users : IdentityUser
    {
       public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Profiles? Profile { get; set; }

    }
}
