
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface ICollegeRepository
    {
        Task<List<College>> GetAllAsync();

        Task<College?> GetByIdAsync(int collegeId);

        Task<College?> GetByCodeAsync(string collegeCode);

        Task AddAsync(College college);

        void Update(College college);

        void Delete(College college);

        Task SaveChangesAsync();
    }
}
