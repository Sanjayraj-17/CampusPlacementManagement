
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
    }
}
