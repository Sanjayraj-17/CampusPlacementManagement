using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync(CompanyStatus? status = null);

        Task<Company?> GetByIdAsync(int companyId);

        Task<Company?> GetByEmailAsync(string email);

        Task<Company?> GetByNameAsync(string name);

        Task AddAsync(Company company);

        void Update(Company company);

        void Delete(Company company);

        Task SaveChangesAsync();
    }
}
