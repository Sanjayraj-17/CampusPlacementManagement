using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IPlacementOfficerRepository
    {
        Task<List<PlacementOfficer>> GetAllAsync(int? collegeId = null);

        Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task AddAsync(PlacementOfficer placementOfficer);

        void Update(PlacementOfficer placementOfficer);

        void Delete(PlacementOfficer placementOfficer);

        Task SaveChangesAsync();
    }
}
