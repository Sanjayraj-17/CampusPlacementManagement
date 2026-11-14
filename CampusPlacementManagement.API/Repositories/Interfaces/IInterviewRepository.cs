using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IInterviewRepository
    {
        // Interview Rounds
        Task<InterviewRound?> GetRoundByIdAsync(int roundId);

        Task<List<InterviewRound>> GetRoundsByJobIdAsync(int jobId);

        Task<InterviewRound?> GetRoundByJobAndNumberAsync(int jobId, int roundNumber);

        Task AddRoundAsync(InterviewRound round);

        void UpdateRound(InterviewRound round);

        void DeleteRound(InterviewRound round);

        // Interview Schedules
        Task<InterviewSchedule?> GetScheduleByIdAsync(int scheduleId);

        Task<List<InterviewSchedule>> GetSchedulesByApplicationIdAsync(int applicationId);

        Task<List<InterviewSchedule>> GetSchedulesByJobIdAsync(int jobId, InterviewScheduleStatus? status = null);

        Task<List<InterviewSchedule>> GetSchedulesByCompanyIdAsync(int companyId, InterviewScheduleStatus? status = null);

        Task<List<InterviewSchedule>> GetSchedulesByCollegeIdAsync(int collegeId, InterviewScheduleStatus? status = null);

        Task<List<InterviewSchedule>> GetSchedulesByStudentIdAsync(int studentId, InterviewScheduleStatus? status = null);

        Task<List<InterviewSchedule>> GetAllSchedulesAsync(InterviewScheduleStatus? status = null);

        Task AddScheduleAsync(InterviewSchedule schedule);

        void UpdateSchedule(InterviewSchedule schedule);

        void DeleteSchedule(InterviewSchedule schedule);

        Task SaveChangesAsync();
    }
}
