using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.CurriculumManagement;
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
        public DbSet<GradeLevels> GradeLevels { get; set; }
        public DbSet<ElectiveCourse> ElectiveCourses { get; set; }
        public DbSet<CompulsoryCourse> CompulsoryCourses { get; set; }
        public DbSet<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseCategory> CourseCategorys { get; set; }

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

            modelBuilder.Entity<Course>()
                .HasOne(c => c.CourseCategory)
                .WithMany(cc => cc.Courses)
                .HasForeignKey(c => c.CourseCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // ป้องกันการลบ Category ที่มี Course อยู่

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasOne(ea => ea.Curriculum)
                .WithMany(c => c.ExtracurricularActivities)
                .HasForeignKey(ea => ea.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade); // ลบกิจกรรมทั้งหมดเมื่อหลักสูตรถูกลบ

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasOne(ea => ea.Activity)
                .WithMany(a => a.ExtracurricularActivities)
                .HasForeignKey(ea => ea.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.GradeLevel)
                .WithMany(gl => gl.ElectiveCourses)
                .HasForeignKey(ec => ec.GradeLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.Curriculum)
                .WithMany(c => c.ElectiveCourses)
                .HasForeignKey(ec => ec.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectiveCourse>()
                .HasOne(ec => ec.Course)
                .WithMany(c => c.ElectiveCourses)
                .HasForeignKey(ec => ec.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.GradeLevel)
                .WithMany(gl => gl.CompulsoryCourses)
                .HasForeignKey(cc => cc.GradeLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.Curriculum)
                .WithMany(c => c.CompulsoryCourses)
                .HasForeignKey(cc => cc.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryCourse>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CompulsoryCourses)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.GradeLevel)
                .WithMany(gl => gl.CompulsoryElectiveCourses)
                .HasForeignKey(cec => cec.GradeLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.Curriculum)
                .WithMany(c => c.CompulsoryElectiveCourses)
                .HasForeignKey(cec => cec.CurriculumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompulsoryElectiveCourse>()
                .HasOne(cec => cec.Course)
                .WithMany(c => c.CompulsoryElectiveCourses)
                .HasForeignKey(cec => cec.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

