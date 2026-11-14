using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class JobOpeningRepository : IJobOpeningRepository
    {
        private readonly ApplicationDbContext _context;

        public JobOpeningRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all jobs with optional filters
        public async Task<List<JobOpening>> GetAllAsync(int? companyId = null, JobStatus? status = null)
        {
            var query = _context.JobOpenings
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(h => h.User)
                .AsNoTracking();

            if (companyId.HasValue)
            {
                query = query.Where(j => j.CompanyId == companyId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(j => j.Status == status.Value);
            }

            return await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        // Get published jobs visible to a specific college
        public async Task<List<JobOpening>> GetPublishedJobsForCollegeAsync(int collegeId)
        {
            var now = DateTime.UtcNow;

            return await _context.JobOpenings
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(h => h.User)
                .AsNoTracking()
                .Where(j =>
                    j.Status == JobStatus.Published &&
                    j.ApplicationDeadline >= now &&
                    (j.IsOpenToAllColleges ||
                     _context.JobTargetColleges.Any(tc => tc.JobId == j.JobId && tc.CollegeId == collegeId)))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        // Get job by ID
        public async Task<JobOpening?> GetByIdAsync(int jobId)
        {
            return await _context.JobOpenings
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        // Add new job
        public async Task AddAsync(JobOpening job)
        {
            await _context.JobOpenings.AddAsync(job);
        }

        // Update job
        public void Update(JobOpening job)
        {
            _context.JobOpenings.Update(job);
        }

        // Delete job
        public void Delete(JobOpening job)
        {
            _context.JobOpenings.Remove(job);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Related Target Colleges
        public async Task AddTargetCollegesAsync(IEnumerable<JobTargetCollege> targetColleges)
        {
            await _context.JobTargetColleges.AddRangeAsync(targetColleges);
        }

        public async Task<List<JobTargetCollege>> GetTargetCollegesByJobIdAsync(int jobId)
        {
            return await _context.JobTargetColleges
                .Include(tc => tc.College)
                .AsNoTracking()
                .Where(tc => tc.JobId == jobId)
                .ToListAsync();
        }

        public void RemoveTargetColleges(IEnumerable<JobTargetCollege> targetColleges)
        {
            _context.JobTargetColleges.RemoveRange(targetColleges);
        }

        // Related Eligible Departments
        public async Task AddEligibleDepartmentsAsync(IEnumerable<JobEligibleDepartment> departments)
        {
            await _context.JobEligibleDepartments.AddRangeAsync(departments);
        }

        public async Task<List<JobEligibleDepartment>> GetEligibleDepartmentsByJobIdAsync(int jobId)
        {
            return await _context.JobEligibleDepartments
                .AsNoTracking()
                .Where(d => d.JobId == jobId)
                .ToListAsync();
        }

        public void RemoveEligibleDepartments(IEnumerable<JobEligibleDepartment> departments)
        {
            _context.JobEligibleDepartments.RemoveRange(departments);
        }

        // Related Eligible Graduation Years
        public async Task AddEligibleGraduationYearsAsync(IEnumerable<JobEligibleGraduationYear> years)
        {
            await _context.JobEligibleGraduationYears.AddRangeAsync(years);
        }

        public async Task<List<JobEligibleGraduationYear>> GetEligibleGraduationYearsByJobIdAsync(int jobId)
        {
            return await _context.JobEligibleGraduationYears
                .AsNoTracking()
                .Where(y => y.JobId == jobId)
                .ToListAsync();
        }

        public void RemoveEligibleGraduationYears(IEnumerable<JobEligibleGraduationYear> years)
        {
            _context.JobEligibleGraduationYears.RemoveRange(years);
        }
    }
}
