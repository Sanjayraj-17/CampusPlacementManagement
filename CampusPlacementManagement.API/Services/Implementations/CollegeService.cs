using CampusPlacementManagement.API.DTOs.College;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _collegeRepository;

        public CollegeService(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }

        // Get all colleges
        public async Task<List<College>> GetAllAsync()
        {
            return await _collegeRepository.GetAllAsync();
        }

        // Get college by ID
        public async Task<College?> GetByIdAsync(int collegeId)
        {
            return await _collegeRepository.GetByIdAsync(collegeId);
        }

        // Create college
        public async Task<string> CreateAsync(CollegeRequestDto request)
        {
            var collegeCode = request.CollegeCode.Trim().ToUpperInvariant();

            // Check duplicate College Code
            var existingCollege =
                await _collegeRepository.GetByCodeAsync(collegeCode);

            if (existingCollege != null)
            {
                return "College code already exists.";
            }

            var college = new College
            {
                CollegeName = request.CollegeName.Trim(),
                CollegeCode = collegeCode,
                UniversityName = request.UniversityName?.Trim(),
                OfficialEmail = request.OfficialEmail.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address.Trim(),
                Website = request.Website?.Trim(),

                // New colleges require SuperAdmin approval
                Status = CollegeStatus.Pending,

                CreatedAt = DateTime.UtcNow
            };

            await _collegeRepository.AddAsync(college);
            await _collegeRepository.SaveChangesAsync();

            return "College created successfully. Awaiting SuperAdmin approval.";
        }

        // Update college
        public async Task<string> UpdateAsync(
            int collegeId,
            CollegeRequestDto request)
        {
            var college =
                await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
            {
                return "College not found.";
            }

            var collegeCode = request.CollegeCode.Trim().ToUpperInvariant();

            var existingCollege =
                await _collegeRepository.GetByCodeAsync(collegeCode);

            if (existingCollege != null &&
                existingCollege.CollegeId != collegeId)
            {
                return "College code already exists.";
            }

            // Update editable information only.
            // Preserve Status and CreatedAt.
            college.CollegeName = request.CollegeName.Trim();
            college.CollegeCode = collegeCode;
            college.UniversityName = request.UniversityName?.Trim();
            college.OfficialEmail = request.OfficialEmail.Trim();
            college.PhoneNumber = request.PhoneNumber.Trim();
            college.Address = request.Address.Trim();
            college.Website = request.Website?.Trim();

            _collegeRepository.Update(college);
            await _collegeRepository.SaveChangesAsync();

            return "College updated successfully.";
        }

        // Delete college
        public async Task<string> DeleteAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return "College not found.";

            try
            {
                _collegeRepository.Delete(college);
                await _collegeRepository.SaveChangesAsync();

                return "College deleted successfully.";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return "College cannot be deleted because related records exist. Deactivate it instead.";
            }
        }

        // Approve college
        public async Task<string> ApproveAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return "College not found.";

            if (college.Status == CollegeStatus.Approved)
                return "College is already approved.";

            college.Status = CollegeStatus.Approved;

            _collegeRepository.Update(college);
            await _collegeRepository.SaveChangesAsync();

            return "College approved successfully.";
        }

        // Reject college
        public async Task<string> RejectAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return "College not found.";

            if (college.Status == CollegeStatus.Rejected)
                return "College is already rejected.";

            college.Status = CollegeStatus.Rejected;

            _collegeRepository.Update(college);
            await _collegeRepository.SaveChangesAsync();

            return "College rejected successfully.";
        }

        // Suspend college
        public async Task<string> SuspendAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return "College not found.";

            if (college.Status == CollegeStatus.Suspended)
                return "College is already suspended.";

            college.Status = CollegeStatus.Suspended;

            _collegeRepository.Update(college);
            await _collegeRepository.SaveChangesAsync();

            return "College suspended successfully.";
        }

        // Reactivate college
        public async Task<string> ReactivateAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return "College not found.";

            if (college.Status == CollegeStatus.Approved)
                return "College is already active.";

            college.Status = CollegeStatus.Approved;

            _collegeRepository.Update(college);
            await _collegeRepository.SaveChangesAsync();

            return "College reactivated successfully.";
        }
    }
}
