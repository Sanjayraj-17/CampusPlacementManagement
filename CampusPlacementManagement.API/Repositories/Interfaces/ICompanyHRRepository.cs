using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface ICompanyHRRepository
    {
        Task<List<CompanyHR>> GetAllAsync(int? companyId = null);

        Task<CompanyHR?> GetByIdAsync(int companyHRId);

        Task<CompanyHR?> GetByUserIdAsync(string userId);

        Task AddAsync(CompanyHR companyHR);

        void Update(CompanyHR companyHR);

        void Delete(CompanyHR companyHR);

        Task SaveChangesAsync();
    }
}
