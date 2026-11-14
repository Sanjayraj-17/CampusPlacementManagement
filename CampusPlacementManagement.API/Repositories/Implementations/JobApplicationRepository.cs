using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve application by ID with related Job, Company, Student, User, and College
        public async Task<JobApplication?> GetByIdAsync(int applicationId)
        {
            return await _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
        }

        // Check if student has already applied for this job
        public async Task<JobApplication?> GetByStudentAndJobAsync(int studentId, int jobId)
        {
            return await _context.JobApplications
                .Include(a => a.JobOpening)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.JobId == jobId);
        }

        // Get all applications submitted by a specific student
        public async Task<List<JobApplication>> GetByStudentIdAsync(int studentId)
        {
            return await _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // Get all applications for a specific job opening (optional status filter)
        public async Task<List<JobApplication>> GetByJobIdAsync(int jobId, ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .Where(a => a.JobId == jobId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // Get all applications for all jobs posted by a company (for Company HR)
        public async Task<List<JobApplication>> GetByCompanyIdAsync(int companyId, ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .Where(a => a.JobOpening.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // Get all applications submitted by students of a college (for Placement Officer)
        public async Task<List<JobApplication>> GetByCollegeIdAsync(int collegeId, ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .Where(a => a.Student.CollegeId == collegeId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // Get all applications across the entire platform (for SuperAdmin)
        public async Task<List<JobApplication>> GetAllAsync(ApplicationStatus? status = null)
        {
            var query = _context.JobApplications
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Student)
                    .ThenInclude(s => s.College)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public void Update(JobApplication application)
        {
            _context.JobApplications.Update(application);
        }

        public async Task AddStatusHistoryAsync(ApplicationStatusHistory history)
        {
            await _context.ApplicationStatusHistories.AddAsync(history);
        }

        public async Task<List<ApplicationStatusHistory>> GetStatusHistoryByApplicationIdAsync(int applicationId)
        {
            return await _context.ApplicationStatusHistories
                .Include(h => h.UpdatedByUser)
                .Where(h => h.ApplicationId == applicationId)
                .OrderBy(h => h.UpdatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
