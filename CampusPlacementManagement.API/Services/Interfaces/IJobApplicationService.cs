using CampusPlacementManagement.API.DTOs.Application;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IJobApplicationService
    {
        // Student applies for a job opening
        Task<string> ApplyAsync(ApplyJobRequestDto request, string currentUserId);

        // Student checks their eligibility for a job opening
        Task<JobEligibilityCheckResultDto?> CheckEligibilityAsync(int jobId, string currentUserId);

        // Student withdraws their job application
        Task<string> WithdrawAsync(int applicationId, string currentUserId);

        // Company HR or SuperAdmin updates application status with an audit log
        Task<string> UpdateStatusAsync(int applicationId, UpdateApplicationStatusDto request, string currentUserId, bool isSuperAdmin);

        // View single application details (resource-authorized)
        Task<JobApplicationResponseDto?> GetByIdAsync(
            int applicationId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent);

        // Student views all their own job applications
        Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(string currentUserId);

        // View applications for a specific job (Company HR for their company, Placement Officer for their college students, SuperAdmin for all)
        Task<List<JobApplicationResponseDto>> GetApplicationsForJobAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            ApplicationStatus? status = null);

        // View all applications accessible by role
        Task<List<JobApplicationResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            ApplicationStatus? status = null);
    }
}
