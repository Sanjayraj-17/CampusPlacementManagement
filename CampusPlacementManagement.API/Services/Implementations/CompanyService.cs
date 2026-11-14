using CampusPlacementManagement.API.DTOs.Company;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        // Get all companies (SuperAdmin can see all or filter by status; other roles see approved companies)
        public async Task<List<Company>> GetAllAsync(CompanyStatus? status = null, bool isSuperAdmin = false)
        {
            if (!isSuperAdmin)
            {
                // Non-admin roles (Students, Placement Officers) should only view Approved companies
                return await _companyRepository.GetAllAsync(CompanyStatus.Approved);
            }

            return await _companyRepository.GetAllAsync(status);
        }

        // Get company by ID
        public async Task<Company?> GetByIdAsync(int companyId)
        {
            return await _companyRepository.GetByIdAsync(companyId);
        }

        // Create new company
        public async Task<string> CreateAsync(CompanyRequestDto request)
        {
            // Check duplicate company email
            var existingByEmail = await _companyRepository.GetByEmailAsync(request.CompanyEmail);
            if (existingByEmail != null)
            {
                return "Company email already exists.";
            }

            // Check duplicate company name
            var existingByName = await _companyRepository.GetByNameAsync(request.CompanyName);
            if (existingByName != null)
            {
                return "Company name already exists.";
            }

            var company = new Company
            {
                CompanyName = request.CompanyName.Trim(),
                CompanyEmail = request.CompanyEmail.Trim(),
                Website = request.Website?.Trim(),
                Industry = request.Industry.Trim(),
                Address = request.Address.Trim(),
                Description = request.Description?.Trim(),

                // New companies require SuperAdmin approval
                Status = CompanyStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();

            return "Company registered successfully. Awaiting SuperAdmin approval.";
        }

        // Update company details (preserves Status and CreatedAt)
        public async Task<string> UpdateAsync(int companyId, CompanyRequestDto request)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            // Check duplicate email on another company
            var existingByEmail = await _companyRepository.GetByEmailAsync(request.CompanyEmail);
            if (existingByEmail != null && existingByEmail.CompanyId != companyId)
            {
                return "Company email already exists.";
            }

            // Check duplicate name on another company
            var existingByName = await _companyRepository.GetByNameAsync(request.CompanyName);
            if (existingByName != null && existingByName.CompanyId != companyId)
            {
                return "Company name already exists.";
            }

            // Update editable properties only
            company.CompanyName = request.CompanyName.Trim();
            company.CompanyEmail = request.CompanyEmail.Trim();
            company.Website = request.Website?.Trim();
            company.Industry = request.Industry.Trim();
            company.Address = request.Address.Trim();
            company.Description = request.Description?.Trim();

            _companyRepository.Update(company);
            await _companyRepository.SaveChangesAsync();

            return "Company updated successfully.";
        }

        // Approve company
        public async Task<string> ApproveAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            if (company.Status == CompanyStatus.Approved)
            {
                return "Company is already approved.";
            }

            company.Status = CompanyStatus.Approved;

            _companyRepository.Update(company);
            await _companyRepository.SaveChangesAsync();

            return "Company approved successfully.";
        }

        // Reject company
        public async Task<string> RejectAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            if (company.Status == CompanyStatus.Rejected)
            {
                return "Company is already rejected.";
            }

            company.Status = CompanyStatus.Rejected;

            _companyRepository.Update(company);
            await _companyRepository.SaveChangesAsync();

            return "Company rejected successfully.";
        }

        // Suspend company
        public async Task<string> SuspendAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            if (company.Status == CompanyStatus.Suspended)
            {
                return "Company is already suspended.";
            }

            company.Status = CompanyStatus.Suspended;

            _companyRepository.Update(company);
            await _companyRepository.SaveChangesAsync();

            return "Company suspended successfully.";
        }

        // Reactivate company
        public async Task<string> ReactivateAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            if (company.Status == CompanyStatus.Approved)
            {
                return "Company is already active.";
            }

            company.Status = CompanyStatus.Approved;

            _companyRepository.Update(company);
            await _companyRepository.SaveChangesAsync();

            return "Company reactivated successfully.";
        }

        // Delete company safely
        public async Task<string> DeleteAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return "Company not found.";
            }

            try
            {
                _companyRepository.Delete(company);
                await _companyRepository.SaveChangesAsync();

                return "Company deleted successfully.";
            }
            catch (DbUpdateException)
            {
                return "Company cannot be deleted because related records exist. Suspend the company instead.";
            }
        }
    }
}
