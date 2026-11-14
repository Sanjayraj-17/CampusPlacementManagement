using CampusPlacementManagement.API.DTOs.CompanyHR;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface ICompanyHRService
    {
        Task<List<CompanyHRResponseDto>> GetAllAsync(int? companyId = null);

        Task<CompanyHRResponseDto?> GetByIdAsync(int companyHRId);

        Task<CompanyHRResponseDto?> GetByUserIdAsync(string userId);

        Task<string> CreateAsync(CreateCompanyHRDto request);

        Task<string> UpdateAsync(
            int companyHRId,
            UpdateCompanyHRDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> ApproveAsync(int companyHRId, string superAdminUserId);

        Task<string> RejectAsync(int companyHRId, string superAdminUserId);

        Task<string> SuspendAsync(int companyHRId);

        Task<string> ReactivateAsync(int companyHRId);

        Task<string> DeleteAsync(int companyHRId);
    }
}
