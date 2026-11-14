using CampusPlacementManagement.API.DTOs.Company;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<List<Company>> GetAllAsync(CompanyStatus? status = null, bool isSuperAdmin = false);

        Task<Company?> GetByIdAsync(int companyId);

        Task<string> CreateAsync(CompanyRequestDto request);

        Task<string> UpdateAsync(int companyId, CompanyRequestDto request);

        Task<string> DeleteAsync(int companyId);

        Task<string> ApproveAsync(int companyId);

        Task<string> RejectAsync(int companyId);

        Task<string> SuspendAsync(int companyId);

        Task<string> ReactivateAsync(int companyId);
    }
}
