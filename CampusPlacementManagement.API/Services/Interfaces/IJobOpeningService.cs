using CampusPlacementManagement.API.DTOs.Job;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IJobOpeningService
    {
        Task<List<JobOpeningResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent,
            JobStatus? status = null);

        Task<JobOpeningResponseDto?> GetByIdAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent);

        Task<string> CreateAsync(
            CreateJobOpeningDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> UpdateAsync(
            int jobId,
            UpdateJobOpeningDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> PublishAsync(int jobId, string currentUserId, bool isSuperAdmin);

        Task<string> CloseAsync(int jobId, string currentUserId, bool isSuperAdmin);

        Task<string> CancelAsync(int jobId, string currentUserId, bool isSuperAdmin);

        Task<string> DeleteAsync(int jobId, string currentUserId, bool isSuperAdmin);
    }
}
