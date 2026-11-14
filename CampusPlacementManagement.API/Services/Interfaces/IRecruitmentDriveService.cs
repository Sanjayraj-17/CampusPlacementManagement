using CampusPlacementManagement.API.DTOs.RecruitmentDrive;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IRecruitmentDriveService
    {
        // Schedule a recruitment drive
        Task<string> CreateAsync(CreateRecruitmentDriveDto request, string currentUserId, bool isSuperAdmin);

        // Update recruitment drive details and status
        Task<string> UpdateAsync(int driveId, UpdateRecruitmentDriveDto request, string currentUserId, bool isSuperAdmin);

        // Delete recruitment drive
        Task<string> DeleteAsync(int driveId, string currentUserId, bool isSuperAdmin);

        // Get single recruitment drive (resource-authorized)
        Task<RecruitmentDriveResponseDto?> GetByIdAsync(
            int driveId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent);

        // Get all recruitment drives (filtered per role)
        Task<List<RecruitmentDriveResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent,
            int? jobId = null,
            int? collegeId = null,
            RecruitmentDriveStatus? status = null);
    }
}
