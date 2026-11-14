using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class RecruitmentDriveRepository : IRecruitmentDriveRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruitmentDriveRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecruitmentDrive?> GetByIdAsync(int driveId)
        {
            return await _context.RecruitmentDrives
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(r => r.College)
                .FirstOrDefaultAsync(r => r.DriveId == driveId);
        }

        public async Task<List<RecruitmentDrive>> GetAllAsync(
            int? companyId = null,
            int? collegeId = null,
            int? jobId = null,
            RecruitmentDriveStatus? status = null)
        {
            var query = _context.RecruitmentDrives
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(r => r.College)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(r => r.JobOpening.CompanyId == companyId.Value);
            }

            if (collegeId.HasValue)
            {
                query = query.Where(r => r.CollegeId == collegeId.Value);
            }

            if (jobId.HasValue)
            {
                query = query.Where(r => r.JobId == jobId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderBy(r => r.DriveDate)
                .ToListAsync();
        }

        public async Task<List<RecruitmentDrive>> GetDrivesForCollegeAsync(int collegeId, RecruitmentDriveStatus? status = null)
        {
            var query = _context.RecruitmentDrives
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(r => r.College)
                .Where(r => r.CollegeId == collegeId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderBy(r => r.DriveDate)
                .ToListAsync();
        }

        public async Task<List<RecruitmentDrive>> GetDrivesForCompanyAsync(int companyId, RecruitmentDriveStatus? status = null)
        {
            var query = _context.RecruitmentDrives
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(r => r.College)
                .Where(r => r.JobOpening.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderBy(r => r.DriveDate)
                .ToListAsync();
        }

        public async Task<List<RecruitmentDrive>> GetDrivesForJobAsync(int jobId, RecruitmentDriveStatus? status = null)
        {
            var query = _context.RecruitmentDrives
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .Include(r => r.College)
                .Where(r => r.JobId == jobId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderBy(r => r.DriveDate)
                .ToListAsync();
        }

        public async Task AddAsync(RecruitmentDrive drive)
        {
            await _context.RecruitmentDrives.AddAsync(drive);
        }

        public void Update(RecruitmentDrive drive)
        {
            _context.RecruitmentDrives.Update(drive);
        }

        public void Delete(RecruitmentDrive drive)
        {
            _context.RecruitmentDrives.Remove(drive);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
