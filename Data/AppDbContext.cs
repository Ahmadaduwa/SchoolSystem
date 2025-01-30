using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models.CurriculumManagement;
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

        public DbSet<Curriculum> Curriculum { get; set; }
        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<SchoolSystem.Models.CurriculumManagement.Activity> Activities { get; set; }
        public DbSet<ExtracurricularActivity> ExtracurricularActivities { get; set; }
        public DbSet<Course> Courses { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Profiles>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profiles>(p => p.UserId);

            ConfigureUserRelationship(modelBuilder);
        }

        private void ConfigureUserRelationship(ModelBuilder modelBuilder)
        {

            

            ////////////////////////////

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasOne(ea => ea.Course)
                .WithMany(c => c.ExtracurricularActivities)
                .HasForeignKey(ea => ea.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasOne(ea => ea.Activity)
                .WithMany(a => a.ExtracurricularActivities)
                .HasForeignKey(ea => ea.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            Console.WriteLine("ConfigureUserRelationship");
        }
    }
}

