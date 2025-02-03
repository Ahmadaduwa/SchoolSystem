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

        /// <summary>
        /// User Management
        /// </summary>
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Profiles> Profiles { get; set; }
        /// <summary>
        /// Class Management
        /// </summary>
        public DbSet<Course> Course { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<ClassManagement> ClassManagements { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; } // Added ClassSchedule
        public DbSet<CourseCategory> SubjectCategories { get; set; } // Added SubjectCategory


        /// <summary>
        /// Attendance Management
        /// </summary>


        /// <summary>
        /// Curriculum Management
        /// </summary>
        public DbSet<Curriculum> Curriculum { get; set; }
        public DbSet<Models.ActivityManagement.Activity> Activities { get; set; }
        public DbSet<ExtracurricularActivity> ExtracurricularActivities { get; set; }
        public DbSet<GradeLevels> GradeLevels { get; set; }
        public DbSet<ElectiveCourse> ElectiveCourses { get; set; }
        public DbSet<CompulsoryCourse> CompulsoryCourses { get; set; }
        public DbSet<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUserRelationship(modelBuilder);
            ConfigureCurriculumManagement(modelBuilder);
            ConfigureClassManagement(modelBuilder);
            ConfigureAttendanceManagement(modelBuilder);

        }
        private void ConfigureUserRelationship(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profiles>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profiles>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ 2. Profile กับ Teacher (1-1)
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.Profile)
                .WithOne(p => p.Teacher) // ✅ ใช้ Navigation Property ให้ถูกต้อง
                .HasForeignKey<Teacher>(t => t.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureClassManagement(ModelBuilder modelBuilder)
        {
            // Configure relationships for ClassManagement
            modelBuilder.Entity<ClassManagement>()
                .HasOne(cm => cm.Class)
                .WithMany(c => c.ClassManagement)
                .HasForeignKey(cm => cm.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassManagement>()
                .HasOne(cm => cm.Teacher)
                .WithMany(t => t.ClassManagements)
                .HasForeignKey(cm => cm.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassManagement>()
                .HasOne(cm => cm.Course)
                .WithMany(c => c.ClassManagements)
                .HasForeignKey(cm => cm.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassManagement>()
                .HasOne(cm => cm.Semester)
                .WithMany(s => s.ClassManagements)
                .HasForeignKey(cm => cm.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassSchedule>()
                .HasOne(cs => cs.ClassManagement)
                .WithMany(cm => cm.ClassSchedules)
                .HasForeignKey(cs => cs.CM_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course>()
                .HasOne(sj => sj.CourseCategory)
                .WithMany(sc => sc.Courses)
                .HasForeignKey(sj => sj.CourseCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureAttendanceManagement(ModelBuilder modelBuilder)
        {
            Debug.WriteLine("Attendance Management");
        }

        private void ConfigureCurriculumManagement(ModelBuilder modelBuilder)
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

