
using CampusPlacementManagement.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // COLLEGE AND USER MANAGEMENT
        // ==========================================

        public DbSet<College> Colleges { get; set; }

        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<CompanyHR> CompanyHRs { get; set; }

        public DbSet<Student> Students { get; set; }


        // ==========================================
        // JOB MANAGEMENT
        // ==========================================

        public DbSet<JobOpening> JobOpenings { get; set; }

        public DbSet<JobTargetCollege> JobTargetColleges { get; set; }

        public DbSet<JobEligibleDepartment> JobEligibleDepartments { get; set; }

        public DbSet<JobEligibleGraduationYear> JobEligibleGraduationYears { get; set; }


        // ==========================================
        // APPLICATION MANAGEMENT
        // ==========================================

        public DbSet<JobApplication> JobApplications { get; set; }

        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }


        // ==========================================
        // RECRUITMENT MANAGEMENT
        // ==========================================

        public DbSet<RecruitmentDrive> RecruitmentDrives { get; set; }

        public DbSet<InterviewRound> InterviewRounds { get; set; }

        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }


        // ==========================================
        // FLUENT API CONFIGURATION
        // ==========================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Preserve ASP.NET Core Identity configuration
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // COLLEGE CONFIGURATION
            // ==========================================

            // CollegeCode must be unique
            modelBuilder.Entity<College>()
                .HasIndex(c => c.CollegeCode)
                .IsUnique();


            // ApplicationUser - PlacementOfficer (1:1)
            modelBuilder.Entity<PlacementOfficer>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PlacementOfficer>(p => p.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ApplicationUser - CompanyHR (1:1)
            modelBuilder.Entity<CompanyHR>()
                .HasOne(h => h.User)
                .WithOne()
                .HasForeignKey<CompanyHR>(h => h.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ApplicationUser - Student (1:1)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // College - PlacementOfficer (One-to-Many)

            modelBuilder.Entity<PlacementOfficer>()
                .HasOne(p => p.College)
                .WithMany()
                .HasForeignKey(p => p.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // College - Student (One-to-Many)

            modelBuilder.Entity<Student>()
                .HasOne(s => s.College)
                .WithMany()
                .HasForeignKey(s => s.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // PlacementOfficer - Student (One-to-Many)

            modelBuilder.Entity<Student>()
                .HasOne(s => s.PlacementOfficer)
                .WithMany()
                .HasForeignKey(s => s.PlacementOfficerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Student - Unique RegisterNumber within College

            modelBuilder.Entity<Student>()
                .HasIndex(s => new { s.CollegeId, s.RegisterNumber })
                .IsUnique();


        }
    }
}
