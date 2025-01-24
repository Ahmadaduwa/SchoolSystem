using System.Diagnostics;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Models;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.SubjectManagement;

namespace SchoolSystem.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets สำหรับแต่ละโมเดล
        public DbSet<Courses> Courses { get; set; }
        public DbSet<GradeLevels> GradeLevels { get; set; }
        public DbSet<Activities> Activities { get; set; }
        public DbSet<SubjectRelationship> SubjectRelationships { get; set; }
        public DbSet<SubjectCategory> SubjectCategories { get; set; }
        public DbSet<ObjectiveSubject> ObjectiveSubjects { get; set; }
        public DbSet<Objectives> Objectives { get; set; }
        public DbSet<ExtracurricularActivity> ExtracurricularActivities { get; set; }
        public DbSet<ElectiveSubject> ElectiveSubjects { get; set; }
        public DbSet<CompulsorySubject> CompulsorySubjects { get; set; }
        public DbSet<CompulsoryElectiveSubject> CompulsoryElectiveSubjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureSubjectRelationship(modelBuilder);


        }

        private void ConfigureSubjectRelationship(ModelBuilder modelBuilder)
        {
            // Subjects and SubjectCategory Relationship
            modelBuilder.Entity<Subjects>()
                .HasOne(s => s.SubjectCategory)
                .WithMany()
                .HasForeignKey(s => s.SubjectCategoryId);

            // SubjectRelationship: AfterSubjectId and BeforeSubjectId
            modelBuilder.Entity<SubjectRelationship>()
                .HasMany(sr => sr.AfterSubject)
                .WithOne()
                .HasForeignKey("AfterSubjectId");

            modelBuilder.Entity<SubjectRelationship>()
                .HasMany(sr => sr.BeforeSubject)
                .WithOne()
                .HasForeignKey("BeforeSubjectId");

            // ObjectiveSubject: Relationships with Subjects and Objectives
            modelBuilder.Entity<ObjectiveSubject>()
                .HasMany(os => os.Subject)
                .WithOne()
                .HasForeignKey(os => os.SubjectId);

            modelBuilder.Entity<ObjectiveSubject>()
                .HasMany(os => os.Objective)
                .WithOne()
                .HasForeignKey(os => os.ObjectiveID);

            // GradeLevels and Courses Relationships
            modelBuilder.Entity<ElectiveSubject>()
                .HasMany(es => es.Course)
                .WithOne()
                .HasForeignKey(es => es.CourseId);

            modelBuilder.Entity<ElectiveSubject>()
                .HasMany(es => es.GradeLevel)
                .WithOne()
                .HasForeignKey(es => es.GradeLevelId);
            modelBuilder.Entity<ElectiveSubject>()
                .HasMany(es => es.Subject)
                .WithOne()
                .HasForeignKey(es => es.SubjectId);

            modelBuilder.Entity<CompulsorySubject>()
                .HasMany(cs => cs.Course)
                .WithOne()
                .HasForeignKey(cs => cs.CourseId);

            modelBuilder.Entity<CompulsorySubject>()
                .HasMany(cs => cs.GradeLevel)
                .WithOne()
                .HasForeignKey(cs => cs.GradeLevelId);

            modelBuilder.Entity<CompulsorySubject>()
                .HasMany(cs => cs.Subject)
                .WithOne()
                .HasForeignKey(cs => cs.SubjectId);

            // CompulsoryElectiveSubject Relationships
            modelBuilder.Entity<CompulsoryElectiveSubject>()
                .HasMany(ces => ces.Course)
                .WithOne()
                .HasForeignKey(ces => ces.CourseId);

            modelBuilder.Entity<CompulsoryElectiveSubject>()
                .HasMany(ces => ces.GradeLevel)
                .WithOne()
                .HasForeignKey(ces => ces.GradeLevelId);

            modelBuilder.Entity<CompulsoryElectiveSubject>()
                .HasMany(ces => ces.Subject)
                .WithOne()
                .HasForeignKey(ces => ces.SubjectId);

            // ExtracurricularActivity Relationships
            modelBuilder.Entity<ExtracurricularActivity>()
                .HasMany(ea => ea.Activity)
                .WithOne()
                .HasForeignKey("ActivityId");

            modelBuilder.Entity<ExtracurricularActivity>()
                .HasMany(ea => ea.Course)
                .WithOne()
                .HasForeignKey("CourseId");


            // Additional Indexing (Optional)
            modelBuilder.Entity<Subjects>()
                .HasIndex(s => s.Subject_Code)
                .IsUnique();

            modelBuilder.Entity<GradeLevels>()
                .HasIndex(gl => gl.Name);

            modelBuilder.Entity<Courses>()
                .HasIndex(c => c.CourseName);
        }
    }
}

