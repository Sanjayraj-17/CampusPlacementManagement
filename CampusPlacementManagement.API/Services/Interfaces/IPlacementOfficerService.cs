using CampusPlacementManagement.API.DTOs.PlacementOfficer;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IPlacementOfficerService
    {
        Task<List<PlacementOfficerResponseDto>> GetAllAsync(int? collegeId = null);

        Task<PlacementOfficerResponseDto?> GetByIdAsync(int placementOfficerId);

        Task<PlacementOfficerResponseDto?> GetByUserIdAsync(string userId);

        Task<string> CreateAsync(CreatePlacementOfficerDto request);

        Task<string> UpdateAsync(
            int placementOfficerId,
            UpdatePlacementOfficerDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> ApproveAsync(int placementOfficerId, string superAdminUserId);

        Task<string> RejectAsync(int placementOfficerId, string superAdminUserId);

        Task<string> SuspendAsync(int placementOfficerId);

        Task<string> ReactivateAsync(int placementOfficerId);

        Task<string> DeleteAsync(int placementOfficerId);
    }
}
