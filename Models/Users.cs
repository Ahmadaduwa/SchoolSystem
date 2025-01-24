using Microsoft.AspNetCore.Identity;
using System;

namespace SchoolSystem.Models
{
    public class Users : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Gender { get; set; } = "Not Specified";

        public string Address { get; set; } = string.Empty;

        private DateOnly _dateOfBirth;
        public DateOnly DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (value == default)
                    throw new ArgumentException("Date of Birth cannot be the default value.");
                _dateOfBirth = value;
            }
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    }
}
