
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

        // COLLEGE AND USER MANAGEMENT
        public DbSet<College> Colleges { get; set; }
        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyHR> CompanyHRs { get; set; }
        public DbSet<Student> Students { get; set; }

        // JOB MANAGEMENT
        public DbSet<JobOpening> JobOpenings { get; set; }
        public DbSet<JobTargetCollege> JobTargetColleges { get; set; }
        public DbSet<JobEligibleDepartment> JobEligibleDepartments { get; set; }
        public DbSet<JobEligibleGraduationYear> JobEligibleGraduationYears { get; set; }

        // APPLICATION MANAGEMENT
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }

        // RECRUITMENT MANAGEMENT
        public DbSet<RecruitmentDrive> RecruitmentDrives { get; set; }
        public DbSet<InterviewRound> InterviewRounds { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. COLLEGE CONFIGURATION
            // ==========================================

            modelBuilder.Entity<College>()
                .HasIndex(c => c.CollegeCode)
                .IsUnique();


            // ==========================================
            // 2. USER AND PLACEMENT OFFICER (1:1)
            // ==========================================

            modelBuilder.Entity<PlacementOfficer>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PlacementOfficer>(p => p.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 3. USER AND COMPANY HR (1:1)
            // ==========================================

            modelBuilder.Entity<CompanyHR>()
                .HasOne(h => h.User)
                .WithOne()
                .HasForeignKey<CompanyHR>(h => h.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 4. USER AND STUDENT (1:1)
            // ==========================================

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 5. COLLEGE AND PLACEMENT OFFICER (1:M)
            // ==========================================

            modelBuilder.Entity<PlacementOfficer>()
                .HasOne(p => p.College)
                .WithMany()
                .HasForeignKey(p => p.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 6. COLLEGE AND STUDENT (1:M)
            // ==========================================

            modelBuilder.Entity<Student>()
                .HasOne(s => s.College)
                .WithMany()
                .HasForeignKey(s => s.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 7. PLACEMENT OFFICER AND STUDENT (1:M)
            // ==========================================

            modelBuilder.Entity<Student>()
                .HasOne(s => s.PlacementOfficer)
                .WithMany()
                .HasForeignKey(s => s.PlacementOfficerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Unique Register Number within a College
            modelBuilder.Entity<Student>()
                .HasIndex(s => new { s.CollegeId, s.RegisterNumber })
                .IsUnique();


            // ==========================================
            // 8. COMPANY AND COMPANY HR (1:M)
            // ==========================================

            modelBuilder.Entity<CompanyHR>()
                .HasOne(h => h.Company)
                .WithMany()
                .HasForeignKey(h => h.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 9. COMPANY HR APPROVED BY USER
            // ==========================================

            modelBuilder.Entity<CompanyHR>()
                .HasOne(h => h.ApprovedByUser)
                .WithMany()
                .HasForeignKey(h => h.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 10. PLACEMENT OFFICER APPROVED BY USER
            // ==========================================

            modelBuilder.Entity<PlacementOfficer>()
                .HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 11. COMPANY AND JOB OPENING (1:M)
            // ==========================================

            modelBuilder.Entity<JobOpening>()
                .HasOne(j => j.Company)
                .WithMany()
                .HasForeignKey(j => j.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 12. COMPANY HR AND JOB OPENING (1:M)
            // ==========================================

            modelBuilder.Entity<JobOpening>()
                .HasOne(j => j.CreatedByHR)
                .WithMany()
                .HasForeignKey(j => j.CreatedByHRId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 13. JOB OPENING AND JOB TARGET COLLEGE (1:M)
            // ==========================================

            modelBuilder.Entity<JobTargetCollege>()
                .HasOne(jtc => jtc.JobOpening)
                .WithMany()
                .HasForeignKey(jtc => jtc.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 14. COLLEGE AND JOB TARGET COLLEGE (1:M)
            // ==========================================

            modelBuilder.Entity<JobTargetCollege>()
                .HasOne(jtc => jtc.College)
                .WithMany()
                .HasForeignKey(jtc => jtc.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Unique Job and College combination
            modelBuilder.Entity<JobTargetCollege>()
                .HasIndex(jtc => new { jtc.JobId, jtc.CollegeId })
                .IsUnique();


            // ==========================================
            // 15. JOB OPENING AND ELIGIBLE DEPARTMENT (1:M)
            // ==========================================

            modelBuilder.Entity<JobEligibleDepartment>()
                .HasOne(d => d.JobOpening)
                .WithMany()
                .HasForeignKey(d => d.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Unique Department per Job
            modelBuilder.Entity<JobEligibleDepartment>()
                .HasIndex(d => new { d.JobId, d.Department })
                .IsUnique();


            // ==========================================
            // 16. JOB OPENING AND ELIGIBLE GRADUATION YEAR (1:M)
            // ==========================================

            modelBuilder.Entity<JobEligibleGraduationYear>()
                .HasOne(y => y.JobOpening)
                .WithMany()
                .HasForeignKey(y => y.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Unique Graduation Year per Job
            modelBuilder.Entity<JobEligibleGraduationYear>()
                .HasIndex(y => new { y.JobId, y.GraduationYear })
                .IsUnique();


            // ==========================================
            // 17. JOB OPENING AND JOB APPLICATION (1:M)
            // ==========================================

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.JobOpening)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 18. STUDENT AND JOB APPLICATION (1:M)
            // ==========================================

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Prevent duplicate applications
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => new { a.StudentId, a.JobId })
                .IsUnique();


            // ==========================================
            // 19. JOB OPENING AND RECRUITMENT DRIVE (1:M)
            // ==========================================

            modelBuilder.Entity<RecruitmentDrive>()
                .HasOne(r => r.JobOpening)
                .WithMany()
                .HasForeignKey(r => r.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 20. COLLEGE AND RECRUITMENT DRIVE (1:M)
            // ==========================================

            modelBuilder.Entity<RecruitmentDrive>()
                .HasOne(r => r.College)
                .WithMany()
                .HasForeignKey(r => r.CollegeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 21. JOB OPENING AND INTERVIEW ROUND (1:M)
            // ==========================================

            modelBuilder.Entity<InterviewRound>()
                .HasOne(r => r.JobOpening)
                .WithMany()
                .HasForeignKey(r => r.JobId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 22. JOB APPLICATION AND INTERVIEW SCHEDULE (1:M)
            // ==========================================

            modelBuilder.Entity<InterviewSchedule>()
                .HasOne(s => s.JobApplication)
                .WithMany()
                .HasForeignKey(s => s.ApplicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 23. INTERVIEW ROUND AND INTERVIEW SCHEDULE (1:M)
            // ==========================================

            modelBuilder.Entity<InterviewSchedule>()
                .HasOne(s => s.InterviewRound)
                .WithMany()
                .HasForeignKey(s => s.InterviewRoundId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 24. JOB APPLICATION AND STATUS HISTORY (1:M)
            // ==========================================

            modelBuilder.Entity<ApplicationStatusHistory>()
                .HasOne(h => h.JobApplication)
                .WithMany()
                .HasForeignKey(h => h.ApplicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 25. APPLICATION USER AND STATUS HISTORY (1:M)
            // ==========================================

            modelBuilder.Entity<ApplicationStatusHistory>()
                .HasOne(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

