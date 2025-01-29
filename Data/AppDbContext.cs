using System.Diagnostics;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.SubjectManagement;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Courses> Courses { get; set; }
        public DbSet<Profiles> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureUserRelationship(modelBuilder);
            


        }
        private void ConfigureUserRelationship(ModelBuilder modelBuilder) {
            
            Console.WriteLine("ConfigureUserRelationship");
        }
    }
}

