using CampusPlacementManagement.API.DTOs.CompanyHR;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class CompanyHRService : ICompanyHRService
    {
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyHRService(
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        // Get all company HRs, optionally filtered by companyId
        public async Task<List<CompanyHRResponseDto>> GetAllAsync(int? companyId = null)
        {
            var hrs = await _companyHRRepository.GetAllAsync(companyId);
            return hrs.Select(MapToResponseDto).ToList();
        }

        // Get company HR by ID
        public async Task<CompanyHRResponseDto?> GetByIdAsync(int companyHRId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            return hr == null ? null : MapToResponseDto(hr);
        }

        // Get company HR by ApplicationUser ID
        public async Task<CompanyHRResponseDto?> GetByUserIdAsync(string userId)
        {
            var hr = await _companyHRRepository.GetByUserIdAsync(userId);
            return hr == null ? null : MapToResponseDto(hr);
        }

        // Create new company HR account and profile
        public async Task<string> CreateAsync(CreateCompanyHRDto request)
        {
            // 1. Verify that the company exists
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                return "Company not found.";
            }

            // 2. Business rule: Company must be approved
            if (company.Status != CompanyStatus.Approved)
            {
                return "Cannot assign HR to an unapproved company. The company must be approved first.";
            }

            // 3. Verify that the email is not already registered
            var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existingUser != null)
            {
                return "Email is already registered.";
            }

            // 4. Create the Identity ApplicationUser account
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return string.Join("; ", createResult.Errors.Select(e => e.Description));
            }

            // 5. Assign the CompanyHR role
            var roleResult = await _userManager.AddToRoleAsync(user, "CompanyHR");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return string.Join("; ", roleResult.Errors.Select(e => e.Description));
            }

            // 6. Create the CompanyHR profile record
            var hr = new CompanyHR
            {
                UserId = user.Id,
                CompanyId = request.CompanyId,
                Designation = request.Designation?.Trim()
            };

            await _companyHRRepository.AddAsync(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR registered successfully. Awaiting SuperAdmin approval.";
        }

        // Update company HR profile
        public async Task<string> UpdateAsync(
            int companyHRId,
            UpdateCompanyHRDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            // Resource authorization: HR can only update their own profile; SuperAdmin can update any
            if (!isSuperAdmin && hr.UserId != currentUserId)
            {
                return "You are not authorized to update this profile.";
            }

            // Update user contact details
            if (hr.User != null)
            {
                hr.User.FullName = request.FullName.Trim();
                hr.User.PhoneNumber = request.PhoneNumber.Trim();
                hr.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(hr.User);
            }

            // Update HR designation
            hr.Designation = request.Designation?.Trim();

            // Only SuperAdmin can reassign an HR to a different company
            if (isSuperAdmin && request.CompanyId.HasValue && request.CompanyId.Value != hr.CompanyId)
            {
                var targetCompany = await _companyRepository.GetByIdAsync(request.CompanyId.Value);
                if (targetCompany == null)
                {
                    return "Target company not found.";
                }

                if (targetCompany.Status != CompanyStatus.Approved)
                {
                    return "Target company is not approved.";
                }

                hr.CompanyId = request.CompanyId.Value;
            }

            _companyHRRepository.Update(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR updated successfully.";
        }

        // Approve company HR
        public async Task<string> ApproveAsync(int companyHRId, string superAdminUserId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            if (hr.User != null && hr.User.AccountStatus == AccountStatus.Approved)
            {
                return "Company HR is already approved.";
            }

            hr.ApprovedBy = superAdminUserId;
            hr.ApprovedAt = DateTime.UtcNow;

            if (hr.User != null)
            {
                hr.User.AccountStatus = AccountStatus.Approved;
                hr.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(hr.User);
            }

            _companyHRRepository.Update(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR approved successfully.";
        }

        // Reject company HR
        public async Task<string> RejectAsync(int companyHRId, string superAdminUserId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            if (hr.User != null && hr.User.AccountStatus == AccountStatus.Rejected)
            {
                return "Company HR is already rejected.";
            }

            hr.ApprovedBy = superAdminUserId;
            hr.ApprovedAt = DateTime.UtcNow;

            if (hr.User != null)
            {
                hr.User.AccountStatus = AccountStatus.Rejected;
                hr.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(hr.User);
            }

            _companyHRRepository.Update(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR rejected successfully.";
        }

        // Suspend company HR
        public async Task<string> SuspendAsync(int companyHRId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            if (hr.User != null && hr.User.AccountStatus == AccountStatus.Suspended)
            {
                return "Company HR is already suspended.";
            }

            if (hr.User != null)
            {
                hr.User.AccountStatus = AccountStatus.Suspended;
                hr.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(hr.User);
            }

            _companyHRRepository.Update(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR suspended successfully.";
        }

        // Reactivate company HR
        public async Task<string> ReactivateAsync(int companyHRId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            if (hr.User != null && hr.User.AccountStatus == AccountStatus.Approved)
            {
                return "Company HR is already active.";
            }

            if (hr.User != null)
            {
                hr.User.AccountStatus = AccountStatus.Approved;
                hr.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(hr.User);
            }

            _companyHRRepository.Update(hr);
            await _companyHRRepository.SaveChangesAsync();

            return "Company HR reactivated successfully.";
        }

        // Delete company HR safely
        public async Task<string> DeleteAsync(int companyHRId)
        {
            var hr = await _companyHRRepository.GetByIdAsync(companyHRId);
            if (hr == null)
            {
                return "Company HR not found.";
            }

            try
            {
                var user = hr.User;
                _companyHRRepository.Delete(hr);
                await _companyHRRepository.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return "Company HR deleted successfully.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return "Company HR cannot be deleted because related job postings exist. Suspend the account instead.";
            }
        }

        // Helper: Map entity to clean response DTO
        private static CompanyHRResponseDto MapToResponseDto(CompanyHR hr)
        {
            return new CompanyHRResponseDto
            {
                CompanyHRId = hr.CompanyHRId,
                UserId = hr.UserId,
                FullName = hr.User?.FullName ?? string.Empty,
                Email = hr.User?.Email ?? string.Empty,
                PhoneNumber = hr.User?.PhoneNumber ?? string.Empty,
                CompanyId = hr.CompanyId,
                CompanyName = hr.Company?.CompanyName ?? string.Empty,
                CompanyEmail = hr.Company?.CompanyEmail ?? string.Empty,
                Designation = hr.Designation,
                Status = hr.User?.AccountStatus ?? AccountStatus.Pending,
                ApprovedBy = hr.ApprovedBy,
                ApprovedByAdminName = hr.ApprovedByUser?.FullName,
                ApprovedAt = hr.ApprovedAt,
                CreatedAt = hr.User?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
