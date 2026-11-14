using CampusPlacementManagement.API.DTOs.Interview;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IInterviewService
    {
        // Interview Round Management
        Task<string> CreateRoundAsync(CreateInterviewRoundDto request, string currentUserId, bool isSuperAdmin);

        Task<string> UpdateRoundAsync(int roundId, UpdateInterviewRoundDto request, string currentUserId, bool isSuperAdmin);

        Task<string> DeleteRoundAsync(int roundId, string currentUserId, bool isSuperAdmin);

        Task<InterviewRoundResponseDto?> GetRoundByIdAsync(int roundId);

        Task<List<InterviewRoundResponseDto>> GetRoundsByJobIdAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent);

        // Interview Schedule Management
        Task<string> ScheduleInterviewAsync(ScheduleInterviewRequestDto request, string currentUserId, bool isSuperAdmin);

        Task<string> UpdateScheduleAsync(int scheduleId, UpdateInterviewScheduleDto request, string currentUserId, bool isSuperAdmin);

        Task<string> DeleteScheduleAsync(int scheduleId, string currentUserId, bool isSuperAdmin);

        Task<InterviewScheduleResponseDto?> GetScheduleByIdAsync(
            int scheduleId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent);

        Task<List<InterviewScheduleResponseDto>> GetMySchedulesAsync(string currentUserId);

        Task<List<InterviewScheduleResponseDto>> GetSchedulesForJobAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            InterviewScheduleStatus? status = null);

        Task<List<InterviewScheduleResponseDto>> GetAllSchedulesAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            InterviewScheduleStatus? status = null);
    }
}
