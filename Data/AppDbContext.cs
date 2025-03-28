using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models.ActivityManagement;
using SchoolSystem.Models.Alert;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.CurriculumManagement;
using SchoolSystem.Models.RegistrationManagement;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Notification
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }
        /// <summary>
        /// User Management
        /// </summary>
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        /// <summary>
        /// Class Management
        /// </summary>
        public DbSet<Course> Course { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<ClassManagement> ClassManagements { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; } 


        /// <summary>
        /// Attendance Management
        /// </summary>
        public DbSet<ActivityAttendance> ActivityAttendances { get; set; }
        public DbSet<ActivityManagement> ActivityManagement { get; set; }
        public DbSet<ActivityAttendanceSummary> ActivityAttendanceSummary { get; set; }
        public DbSet<ActivityAttendanceCheck> ActivityAttendanceCheck { get; set; }
        public DbSet<ClassAttendanceSummary> ClassAttendanceSummary { get; set; }
        public DbSet<ClassAttendance> ClassAttendance { get; set; }
        public DbSet<ClassAttendanceCheck> ClassAttendanceCheck { get; set; }

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
            ConfigureRegistered(modelBuilder);
            ConfigureNotification(modelBuilder);

        }
        private void ConfigureNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(a => a.Profile)
                    .WithMany(s => s.Notification)
                    .HasForeignKey(a => a.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

            });
        }
        private void ConfigureRegistered(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegisteredCourse>(entity =>
            {
                entity.HasOne(a => a.Course)
                    .WithMany(s => s.RegisteredCourse)
                    .HasForeignKey(a => a.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Student)
                    .WithMany(s => s.RegisteredCourse)
                    .HasForeignKey(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Semester)
                    .WithMany(a => a.RegisteredCourse)
                    .HasForeignKey(s => s.SemesterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
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

            modelBuilder.Entity<Teacher>()
                .HasOne(a => a.Department)
                .WithMany(s => s.Teachers)
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Teacher>()
                .Property(t => t.Salary)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(t => t.Profile)
                    .WithOne(p => p.Student)
                    .HasForeignKey<Student>(t => t.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(s => s.GPA)
                      .HasPrecision(3, 2); // จำกัดทศนิยมของ GPA เป็น xx.xx
            });
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
            // กำหนด ActivityManagement Table
            modelBuilder.Entity<ActivityManagement>(entity =>
            {
                entity.HasOne(a => a.Activity)
                    .WithMany(s => s.ActivityManagement)
                    .HasForeignKey(a => a.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Semester)
                    .WithMany(a => a.ActivityManagement)
                    .HasForeignKey(s => s.SemesterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ActivitySchedule>(entity =>
            {
                entity.HasOne(a => a.ActivityManagement)
                    .WithMany(s => s.ActivitySchedule)
                    .HasForeignKey(a => a.AM_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // กำหนด ActivityAttendance Table
            modelBuilder.Entity<ActivityAttendance>(entity =>
            {
                entity.HasKey(aa => aa.AA_id); // Primary Key

                entity.HasOne(aa => aa.Student)
                      .WithMany(s => s.ActivityAttendance)
                      .HasForeignKey(aa => aa.Student_id)
                      .OnDelete(DeleteBehavior.Cascade); // ลบ Student แล้วลบ ActivityAttendance ด้วย

                entity.HasOne(aa => aa.ActivityManagement)
                      .WithMany(s => s.ActivityAttendance)
                      .HasForeignKey(aa => aa.AM_id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(aa => aa.Status)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(aa => aa.TimeStamp)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<ActivityAttendanceSummary>(entity =>
            {
                entity.HasKey(aa => aa.AAM_Id); // Primary Key

                entity.HasOne(aa => aa.Student)
                      .WithMany(s => s.AcitivityAttendanceSummary)
                      .HasForeignKey(aa => aa.StudentId)
                      .OnDelete(DeleteBehavior.Cascade); // ลบ Student แล้วลบ ActivityAttendance ด้วย

                entity.HasOne(aa => aa.ActivityManagement)
                      .WithMany(s => s.AcitivityAttendanceSummary)
                      .HasForeignKey(aa => aa.AM_id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClassAttendanceSummary>(entity =>
            {
                entity.HasKey(aa => aa.SAM_Id); // Primary Key

                entity.HasOne(aa => aa.Student)
                      .WithMany(s => s.ClassAttendanceSummary)
                      .HasForeignKey(aa => aa.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(aa => aa.ClassManagement)
                      .WithMany(s => s.ClassAttendanceSummary)
                      .HasForeignKey(aa => aa.CM_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // กำหนด ClassAttendance Table
            modelBuilder.Entity<ClassAttendance>(entity =>
            {
                entity.HasKey(ca => ca.ClassAttendanceId); // Primary Key

                entity.HasOne(ca => ca.Student)
                      .WithMany(s => s.ClassAttendance)
                      .HasForeignKey(ca => ca.StudentId)
                      .OnDelete(DeleteBehavior.Cascade); // ลบ Student แล้วลบ ClassAttendance ด้วย

                entity.HasOne(ca => ca.ClassManagement)
                      .WithMany(s => s.ClassAttendance)
                      .HasForeignKey(ca => ca.CM_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ca => ca.Status)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(ca => ca.Date)
                      .IsRequired();
            });

        
            modelBuilder.Entity<ClassAttendanceCheck>(entity =>
            {
                entity.HasOne(a => a.ClassManagement)
                    .WithMany(cm => cm.ClassAttendanceCheck)
                    .HasForeignKey(a => a.CM_Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<ActivityAttendanceCheck>(entity =>
            {
                entity.HasOne(a => a.ActivityManagement)
                    .WithMany(aa => aa.ActivityAttendanceCheck)
                    .HasForeignKey(a => a.AM_Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasOne(a => a.Class)
                    .WithMany(s => s.Student)
                    .HasForeignKey(a => a.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

           
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

