using CampusPlacementManagement.API.DTOs.RecruitmentDrive;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class RecruitmentDriveService : IRecruitmentDriveService
    {
        private readonly IRecruitmentDriveRepository _driveRepository;
        private readonly IJobOpeningRepository _jobOpeningRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly IStudentRepository _studentRepository;

        public RecruitmentDriveService(
            IRecruitmentDriveRepository driveRepository,
            IJobOpeningRepository jobOpeningRepository,
            ICollegeRepository collegeRepository,
            ICompanyHRRepository companyHRRepository,
            IPlacementOfficerRepository officerRepository,
            IStudentRepository studentRepository)
        {
            _driveRepository = driveRepository;
            _jobOpeningRepository = jobOpeningRepository;
            _collegeRepository = collegeRepository;
            _companyHRRepository = companyHRRepository;
            _officerRepository = officerRepository;
            _studentRepository = studentRepository;
        }

        // Schedule a recruitment drive
        public async Task<string> CreateAsync(CreateRecruitmentDriveDto request, string currentUserId, bool isSuperAdmin)
        {
            // 1. Authorize Company HR
            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return "Company HR profile not found.";
                }

                if (hr.User?.AccountStatus != AccountStatus.Approved)
                {
                    return "Your Company HR account is not approved yet.";
                }

                if (hr.Company.Status != CompanyStatus.Approved)
                {
                    return "Your company is not approved to conduct recruitment drives.";
                }

                // Check job ownership
                var jobOpening = await _jobOpeningRepository.GetByIdAsync(request.JobId);
                if (jobOpening == null)
                {
                    return "Job opening not found.";
                }

                if (jobOpening.CompanyId != hr.CompanyId)
                {
                    return "You are not authorized to create recruitment drives for another company's job opening.";
                }

                if (jobOpening.Status != JobStatus.Published)
                {
                    return $"Recruitment drives can only be scheduled for published job openings (Current Status: {jobOpening.Status}).";
                }
            }
            else
            {
                var jobOpening = await _jobOpeningRepository.GetByIdAsync(request.JobId);
                if (jobOpening == null)
                {
                    return "Job opening not found.";
                }

                if (jobOpening.Status != JobStatus.Published)
                {
                    return $"Recruitment drives can only be scheduled for published job openings (Current Status: {jobOpening.Status}).";
                }
            }

            var job = await _jobOpeningRepository.GetByIdAsync(request.JobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            // 2. Validate College
            var college = await _collegeRepository.GetByIdAsync(request.CollegeId);
            if (college == null)
            {
                return "College not found.";
            }

            if (college.Status != CollegeStatus.Approved)
            {
                return $"College '{college.CollegeName}' is not approved for placement activities (Current Status: {college.Status}).";
            }

            // 3. Validate College Targeting
            if (!job.IsOpenToAllColleges)
            {
                var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(job.JobId);
                var isTargeted = targetColleges.Any(tc => tc.CollegeId == request.CollegeId);
                if (!isTargeted)
                {
                    return $"College '{college.CollegeName}' is not targeted by this job opening.";
                }
            }

            // 4. Validate Drive Date
            if (request.DriveDate < DateTime.UtcNow.Date)
            {
                return "Recruitment drive date cannot be scheduled in the past.";
            }

            // 5. Create Entity
            var drive = new RecruitmentDrive
            {
                JobId = request.JobId,
                CollegeId = request.CollegeId,
                DriveDate = request.DriveDate,
                Venue = request.Venue?.Trim(),
                Description = request.Description?.Trim(),
                Status = RecruitmentDriveStatus.Scheduled
            };

            await _driveRepository.AddAsync(drive);
            await _driveRepository.SaveChangesAsync();

            return $"Recruitment drive scheduled successfully with Drive ID: {drive.DriveId}.";
        }

        // Update recruitment drive details and status
        public async Task<string> UpdateAsync(int driveId, UpdateRecruitmentDriveDto request, string currentUserId, bool isSuperAdmin)
        {
            var drive = await _driveRepository.GetByIdAsync(driveId);
            if (drive == null)
            {
                return "Recruitment drive not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
                {
                    return "You are not authorized to update recruitment drives for another company.";
                }
            }

            drive.DriveDate = request.DriveDate;
            drive.Venue = request.Venue?.Trim();
            drive.Description = request.Description?.Trim();
            drive.Status = request.Status;

            _driveRepository.Update(drive);
            await _driveRepository.SaveChangesAsync();

            return $"Recruitment drive updated successfully (Status: {drive.Status}).";
        }

        // Delete recruitment drive
        public async Task<string> DeleteAsync(int driveId, string currentUserId, bool isSuperAdmin)
        {
            var drive = await _driveRepository.GetByIdAsync(driveId);
            if (drive == null)
            {
                return "Recruitment drive not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
                {
                    return "You are not authorized to delete recruitment drives for another company.";
                }
            }

            if (drive.Status == RecruitmentDriveStatus.Completed)
            {
                return "Cannot delete a completed recruitment drive.";
            }

            _driveRepository.Delete(drive);
            await _driveRepository.SaveChangesAsync();

            return "Recruitment drive deleted successfully.";
        }

        // Get single recruitment drive (resource-authorized)
        public async Task<RecruitmentDriveResponseDto?> GetByIdAsync(
            int driveId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var drive = await _driveRepository.GetByIdAsync(driveId);
            if (drive == null)
            {
                return null;
            }

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
                {
                    return null;
                }
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != drive.CollegeId)
                {
                    return null;
                }
            }
            else if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null || student.CollegeId != drive.CollegeId)
                {
                    return null;
                }
            }

            return MapToResponseDto(drive);
        }

        // Get all recruitment drives (filtered per role)
        public async Task<List<RecruitmentDriveResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent,
            int? jobId = null,
            int? collegeId = null,
            RecruitmentDriveStatus? status = null)
        {
            List<RecruitmentDrive> drives;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return new List<RecruitmentDriveResponseDto>();
                }

                drives = await _driveRepository.GetAllAsync(hr.CompanyId, collegeId, jobId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<RecruitmentDriveResponseDto>();
                }

                drives = await _driveRepository.GetAllAsync(null, officer.CollegeId, jobId, status);
            }
            else if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null)
                {
                    return new List<RecruitmentDriveResponseDto>();
                }

                drives = await _driveRepository.GetDrivesForCollegeAsync(student.CollegeId, status);
            }
            else if (isSuperAdmin)
            {
                drives = await _driveRepository.GetAllAsync(null, collegeId, jobId, status);
            }
            else
            {
                return new List<RecruitmentDriveResponseDto>();
            }

            return drives.Select(MapToResponseDto).ToList();
        }

        private static RecruitmentDriveResponseDto MapToResponseDto(RecruitmentDrive drive)
        {
            return new RecruitmentDriveResponseDto
            {
                DriveId = drive.DriveId,
                JobId = drive.JobId,
                JobTitle = drive.JobOpening?.JobTitle ?? string.Empty,
                JobType = drive.JobOpening?.JobType ?? string.Empty,
                JobLocation = drive.JobOpening?.Location ?? string.Empty,
                CompanyId = drive.JobOpening?.CompanyId ?? 0,
                CompanyName = drive.JobOpening?.Company?.CompanyName ?? string.Empty,
                CompanyEmail = drive.JobOpening?.Company?.CompanyEmail ?? string.Empty,
                CollegeId = drive.CollegeId,
                CollegeName = drive.College?.CollegeName ?? string.Empty,
                CollegeCode = drive.College?.CollegeCode ?? string.Empty,
                DriveDate = drive.DriveDate,
                Venue = drive.Venue,
                Description = drive.Description,
                Status = drive.Status
            };
        }
    }
}
