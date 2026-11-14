using CampusPlacementManagement.API.DTOs.PlacementOfficer;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class PlacementOfficerService : IPlacementOfficerService
    {
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlacementOfficerService(
            IPlacementOfficerRepository officerRepository,
            ICollegeRepository collegeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _officerRepository = officerRepository;
            _collegeRepository = collegeRepository;
            _userManager = userManager;
        }

        // Get all placement officers, optionally filtered by college
        public async Task<List<PlacementOfficerResponseDto>> GetAllAsync(int? collegeId = null)
        {
            var officers = await _officerRepository.GetAllAsync(collegeId);
            return officers.Select(MapToResponseDto).ToList();
        }

        // Get placement officer by ID
        public async Task<PlacementOfficerResponseDto?> GetByIdAsync(int placementOfficerId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            return officer == null ? null : MapToResponseDto(officer);
        }

        // Get placement officer by ApplicationUser ID
        public async Task<PlacementOfficerResponseDto?> GetByUserIdAsync(string userId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(userId);
            return officer == null ? null : MapToResponseDto(officer);
        }

        // Create new placement officer
        public async Task<string> CreateAsync(CreatePlacementOfficerDto request)
        {
            // 1. Verify that the college exists
            var college = await _collegeRepository.GetByIdAsync(request.CollegeId);
            if (college == null)
            {
                return "College not found.";
            }

            // 2. Business rule: College must be approved
            if (college.Status != CollegeStatus.Approved)
            {
                return "Cannot assign a placement officer to an unapproved college. The college must be approved first.";
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

            // 5. Assign the PlacementOfficer role
            var roleResult = await _userManager.AddToRoleAsync(user, "PlacementOfficer");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return string.Join("; ", roleResult.Errors.Select(e => e.Description));
            }

            // 6. Create the PlacementOfficer profile record linked to the user and college
            var officer = new PlacementOfficer
            {
                UserId = user.Id,
                CollegeId = request.CollegeId,
                Designation = request.Designation.Trim(),
                Status = AccountStatus.Pending
            };

            await _officerRepository.AddAsync(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer registered successfully. Awaiting SuperAdmin approval.";
        }

        // Update placement officer profile
        public async Task<string> UpdateAsync(
            int placementOfficerId,
            UpdatePlacementOfficerDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            // Resource authorization: Officer can only update their own profile; SuperAdmin can update any
            if (!isSuperAdmin && officer.UserId != currentUserId)
            {
                return "You are not authorized to update this profile.";
            }

            // Update user contact details
            if (officer.User != null)
            {
                officer.User.FullName = request.FullName.Trim();
                officer.User.PhoneNumber = request.PhoneNumber.Trim();
                officer.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(officer.User);
            }

            // Update officer designation
            officer.Designation = request.Designation.Trim();

            // Only SuperAdmin can reassign an officer to a different college
            if (isSuperAdmin && request.CollegeId.HasValue && request.CollegeId.Value != officer.CollegeId)
            {
                var targetCollege = await _collegeRepository.GetByIdAsync(request.CollegeId.Value);
                if (targetCollege == null)
                {
                    return "Target college not found.";
                }

                if (targetCollege.Status != CollegeStatus.Approved)
                {
                    return "Target college is not approved.";
                }

                officer.CollegeId = request.CollegeId.Value;
            }

            _officerRepository.Update(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer updated successfully.";
        }

        // Approve placement officer
        public async Task<string> ApproveAsync(int placementOfficerId, string superAdminUserId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            if (officer.Status == AccountStatus.Approved)
            {
                return "Placement officer is already approved.";
            }

            officer.Status = AccountStatus.Approved;
            officer.ApprovedBy = superAdminUserId;
            officer.ApprovedAt = DateTime.UtcNow;

            if (officer.User != null)
            {
                officer.User.AccountStatus = AccountStatus.Approved;
                officer.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(officer.User);
            }

            _officerRepository.Update(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer approved successfully.";
        }

        // Reject placement officer
        public async Task<string> RejectAsync(int placementOfficerId, string superAdminUserId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            if (officer.Status == AccountStatus.Rejected)
            {
                return "Placement officer is already rejected.";
            }

            officer.Status = AccountStatus.Rejected;
            officer.ApprovedBy = superAdminUserId;
            officer.ApprovedAt = DateTime.UtcNow;

            if (officer.User != null)
            {
                officer.User.AccountStatus = AccountStatus.Rejected;
                officer.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(officer.User);
            }

            _officerRepository.Update(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer rejected successfully.";
        }

        // Suspend placement officer
        public async Task<string> SuspendAsync(int placementOfficerId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            if (officer.Status == AccountStatus.Suspended)
            {
                return "Placement officer is already suspended.";
            }

            officer.Status = AccountStatus.Suspended;

            if (officer.User != null)
            {
                officer.User.AccountStatus = AccountStatus.Suspended;
                officer.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(officer.User);
            }

            _officerRepository.Update(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer suspended successfully.";
        }

        // Reactivate placement officer
        public async Task<string> ReactivateAsync(int placementOfficerId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            if (officer.Status == AccountStatus.Approved)
            {
                return "Placement officer is already active.";
            }

            officer.Status = AccountStatus.Approved;

            if (officer.User != null)
            {
                officer.User.AccountStatus = AccountStatus.Approved;
                officer.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(officer.User);
            }

            _officerRepository.Update(officer);
            await _officerRepository.SaveChangesAsync();

            return "Placement officer reactivated successfully.";
        }

        // Delete placement officer safely
        public async Task<string> DeleteAsync(int placementOfficerId)
        {
            var officer = await _officerRepository.GetByIdAsync(placementOfficerId);
            if (officer == null)
            {
                return "Placement officer not found.";
            }

            try
            {
                var user = officer.User;
                _officerRepository.Delete(officer);
                await _officerRepository.SaveChangesAsync();

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                return "Placement officer deleted successfully.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return "Placement officer cannot be deleted because related records exist. Suspend the account instead.";
            }
        }

        // Helper: Map entity to clean response DTO
        private static PlacementOfficerResponseDto MapToResponseDto(PlacementOfficer officer)
        {
            return new PlacementOfficerResponseDto
            {
                PlacementOfficerId = officer.PlacementOfficerId,
                UserId = officer.UserId,
                FullName = officer.User?.FullName ?? string.Empty,
                Email = officer.User?.Email ?? string.Empty,
                PhoneNumber = officer.User?.PhoneNumber ?? string.Empty,
                CollegeId = officer.CollegeId,
                CollegeName = officer.College?.CollegeName ?? string.Empty,
                CollegeCode = officer.College?.CollegeCode ?? string.Empty,
                Designation = officer.Designation,
                Status = officer.Status,
                ApprovedBy = officer.ApprovedBy,
                ApprovedByAdminName = officer.ApprovedByUser?.FullName,
                ApprovedAt = officer.ApprovedAt,
                CreatedAt = officer.User?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
