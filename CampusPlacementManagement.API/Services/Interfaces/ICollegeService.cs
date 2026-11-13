using CampusPlacementManagement.API.DTOs.College;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface ICollegeService
    {
        Task<List<College>> GetAllAsync();

        Task<College?> GetByIdAsync(int collegeId);

        Task<string> CreateAsync(CollegeRequestDto request);

        Task<string> UpdateAsync(
            int collegeId,
            CollegeRequestDto request);

        Task<string> DeleteAsync(int collegeId);

        Task<string> ApproveAsync(int collegeId);

        Task<string> RejectAsync(int collegeId);

        Task<string> SuspendAsync(int collegeId);

        Task<string> ReactivateAsync(int collegeId);
    }
}
