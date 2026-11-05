

using CampusPlacementManagement.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<College> Colleges { get; set; }

        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<CompanyHR> CompanyHRs { get; set; }

        public DbSet<Student> Students { get; set; }
        public DbSet<JobOpening> JobOpenings { get; set; }
        public DbSet<JobTargetCollege> JobTargetColleges { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<RecruitmentDrive> RecruitmentDrives { get; set; }
        public DbSet<InterviewRound> InterviewRounds { get; set; }

    }
}
