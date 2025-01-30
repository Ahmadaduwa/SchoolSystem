using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models.CurriculumManagement;
using SchoolSystem.Models.SubjectManagement;
using SchoolSystem.Models.UserManagement;
using SchoolSystem.Models.ClassManagement;

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
        public DbSet<CompulsoryCourse> CompulsoryCourses { get; set; }
        public DbSet<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; }
        public DbSet<ElectiveCourse> ElectiveCourses { get; set; }
        public DbSet<GradeLevels> GradeLevels { get; set; }


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

            // 🔹 CompulsoryCourse Configuration
            modelBuilder.Entity<CompulsoryCourse>()
                .HasKey(cc => cc.CS_Id); // Primary Key

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.Curriculum)
                .WithMany()
                .HasForeignKey(cc => cc.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.Course)
                .WithMany()
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.GradeLevel)
                .WithMany()
                .HasForeignKey(cc => cc.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);


            // 🔹 CompulsoryElectiveCourse Configuration
            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasKey(cec => cec.CES_Id); // Primary Key

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.Course)
                .WithMany()
                .HasForeignKey(cec => cec.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.Course)
                .WithMany()
                .HasForeignKey(cec => cec.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.GradeLevel)
                .WithMany()
                .HasForeignKey(cec => cec.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);


            // 🔹 ElectiveCourse Configuration
            modelBuilder.Entity<ElectiveCourse>()
                .HasKey(ec => ec.ES_Id); // Primary Key

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.Curriculum)
                .WithMany()
                .HasForeignKey(ec => ec.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.Course)
                .WithMany()
                .HasForeignKey(ec => ec.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.GradeLevel)
                .WithMany()
                .HasForeignKey(ec => ec.GradeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            ////////////////////////////

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasOne(ea => ea.Curriculum)
                .WithMany(c => c.ExtracurricularActivities)
                .HasForeignKey(ea => ea.CurriculumId)
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

