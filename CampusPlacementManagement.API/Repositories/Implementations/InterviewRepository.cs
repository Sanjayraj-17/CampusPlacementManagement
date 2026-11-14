using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INTERVIEW ROUND OPERATIONS
        // ==========================================

        public async Task<InterviewRound?> GetRoundByIdAsync(int roundId)
        {
            return await _context.InterviewRounds
                .Include(r => r.JobOpening)
                    .ThenInclude(j => j.Company)
                .FirstOrDefaultAsync(r => r.InterviewRoundId == roundId);
        }

        public async Task<List<InterviewRound>> GetRoundsByJobIdAsync(int jobId)
        {
            return await _context.InterviewRounds
                .Include(r => r.JobOpening)
                .Where(r => r.JobId == jobId)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }

        public async Task<InterviewRound?> GetRoundByJobAndNumberAsync(int jobId, int roundNumber)
        {
            return await _context.InterviewRounds
                .FirstOrDefaultAsync(r => r.JobId == jobId && r.RoundNumber == roundNumber);
        }

        public async Task AddRoundAsync(InterviewRound round)
        {
            await _context.InterviewRounds.AddAsync(round);
        }

        public void UpdateRound(InterviewRound round)
        {
            _context.InterviewRounds.Update(round);
        }

        public void DeleteRound(InterviewRound round)
        {
            _context.InterviewRounds.Remove(round);
        }

        // ==========================================
        // INTERVIEW SCHEDULE OPERATIONS
        // ==========================================

        public async Task<InterviewSchedule?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .FirstOrDefaultAsync(s => s.InterviewScheduleId == scheduleId);
        }

        public async Task<List<InterviewSchedule>> GetSchedulesByApplicationIdAsync(int applicationId)
        {
            return await _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .Where(s => s.ApplicationId == applicationId)
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSchedule>> GetSchedulesByJobIdAsync(int jobId, InterviewScheduleStatus? status = null)
        {
            var query = _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .Where(s => s.JobApplication.JobId == jobId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSchedule>> GetSchedulesByCompanyIdAsync(int companyId, InterviewScheduleStatus? status = null)
        {
            var query = _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .Where(s => s.JobApplication.JobOpening.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSchedule>> GetSchedulesByCollegeIdAsync(int collegeId, InterviewScheduleStatus? status = null)
        {
            var query = _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .Where(s => s.JobApplication.Student.CollegeId == collegeId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSchedule>> GetSchedulesByStudentIdAsync(int studentId, InterviewScheduleStatus? status = null)
        {
            var query = _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .Where(s => s.JobApplication.StudentId == studentId);

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task<List<InterviewSchedule>> GetAllSchedulesAsync(InterviewScheduleStatus? status = null)
        {
            var query = _context.InterviewSchedules
                .Include(s => s.InterviewRound)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Company)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.User)
                .Include(s => s.JobApplication)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(st => st.College)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();
        }

        public async Task AddScheduleAsync(InterviewSchedule schedule)
        {
            await _context.InterviewSchedules.AddAsync(schedule);
        }

        public void UpdateSchedule(InterviewSchedule schedule)
        {
            _context.InterviewSchedules.Update(schedule);
        }

        public void DeleteSchedule(InterviewSchedule schedule)
        {
            _context.InterviewSchedules.Remove(schedule);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
