using CampusPlacementManagement.API.DTOs.Job;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class JobOpeningService : IJobOpeningService
    {
        private readonly IJobOpeningRepository _jobRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly IStudentRepository _studentRepository;

        public JobOpeningService(
            IJobOpeningRepository jobRepository,
            ICompanyRepository companyRepository,
            ICompanyHRRepository companyHRRepository,
            ICollegeRepository collegeRepository,
            IPlacementOfficerRepository officerRepository,
            IStudentRepository studentRepository)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _companyHRRepository = companyHRRepository;
            _collegeRepository = collegeRepository;
            _officerRepository = officerRepository;
            _studentRepository = studentRepository;
        }

        // Get all jobs with role-based filtering
        public async Task<List<JobOpeningResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent,
            JobStatus? status = null)
        {
            List<JobOpening> jobs;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return new List<JobOpeningResponseDto>();
                }

                // Company HR sees all jobs created by their company
                jobs = await _jobRepository.GetAllAsync(hr.CompanyId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<JobOpeningResponseDto>();
                }

                // Placement officer sees published jobs targeting their college
                jobs = await _jobRepository.GetPublishedJobsForCollegeAsync(officer.CollegeId);
            }
            else if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null)
                {
                    return new List<JobOpeningResponseDto>();
                }

                // Student sees published jobs targeting their college
                jobs = await _jobRepository.GetPublishedJobsForCollegeAsync(student.CollegeId);
            }
            else
            {
                // SuperAdmin sees all jobs
                jobs = await _jobRepository.GetAllAsync(null, status);
            }

            var result = new List<JobOpeningResponseDto>();
            foreach (var job in jobs)
            {
                result.Add(await MapToResponseDtoAsync(job));
            }

            return result;
        }

        // Get job by ID with role-based visibility check
        public async Task<JobOpeningResponseDto?> GetByIdAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return null;
            }

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return null;
                }
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return null;
                }

                var targetColleges = await _jobRepository.GetTargetCollegesByJobIdAsync(jobId);
                var isTargeted = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == officer.CollegeId);

                if (!isTargeted)
                {
                    return null;
                }
            }
            else if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null)
                {
                    return null;
                }

                if (job.Status != JobStatus.Published)
                {
                    return null;
                }

                var targetColleges = await _jobRepository.GetTargetCollegesByJobIdAsync(jobId);
                var isTargeted = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == student.CollegeId);

                if (!isTargeted)
                {
                    return null;
                }
            }

            return await MapToResponseDtoAsync(job);
        }

        // Create new job opening
        public async Task<string> CreateAsync(
            CreateJobOpeningDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            // 1. Validate application deadline
            if (request.ApplicationDeadline <= DateTime.UtcNow)
            {
                return "Application deadline must be a future date.";
            }

            // 2. Validate target colleges when not open to all
            if (!request.IsOpenToAllColleges)
            {
                if (request.TargetCollegeIds == null || !request.TargetCollegeIds.Any())
                {
                    return "Please select at least one target college or enable open to all colleges.";
                }

                foreach (var collegeId in request.TargetCollegeIds.Distinct())
                {
                    var college = await _collegeRepository.GetByIdAsync(collegeId);
                    if (college == null)
                    {
                        return $"Target college with ID {collegeId} not found.";
                    }

                    if (college.Status != CollegeStatus.Approved)
                    {
                        return $"Target college '{college.CollegeName}' is not approved. Cannot target unapproved colleges.";
                    }
                }
            }

            // 3. Resolve CompanyId and CreatedByHRId
            int targetCompanyId;
            int targetHRId;

            if (isSuperAdmin)
            {
                if (!request.CompanyId.HasValue || !request.CreatedByHRId.HasValue)
                {
                    return "CompanyId and CreatedByHRId are required for SuperAdmin.";
                }

                var company = await _companyRepository.GetByIdAsync(request.CompanyId.Value);
                if (company == null)
                {
                    return "Company not found.";
                }

                if (company.Status != CompanyStatus.Approved)
                {
                    return "Cannot create job opening for an unapproved company.";
                }

                var hr = await _companyHRRepository.GetByIdAsync(request.CreatedByHRId.Value);
                if (hr == null)
                {
                    return "Company HR not found.";
                }

                if (hr.CompanyId != request.CompanyId.Value)
                {
                    return "The specified HR user does not belong to the selected company.";
                }

                targetCompanyId = request.CompanyId.Value;
                targetHRId = request.CreatedByHRId.Value;
            }
            else
            {
                // Company HR is creating
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return "Company HR profile not found.";
                }

                if (hr.User?.AccountStatus != AccountStatus.Approved)
                {
                    return "Your Company HR account is not approved.";
                }

                var company = await _companyRepository.GetByIdAsync(hr.CompanyId);
                if (company == null || company.Status != CompanyStatus.Approved)
                {
                    return "Your company is not approved. Only approved companies can publish job openings.";
                }

                targetCompanyId = hr.CompanyId;
                targetHRId = hr.CompanyHRId;
            }

            // 4. Create JobOpening entity (defaults to Draft)
            var job = new JobOpening
            {
                CompanyId = targetCompanyId,
                CreatedByHRId = targetHRId,
                JobTitle = request.JobTitle.Trim(),
                JobDescription = request.JobDescription.Trim(),
                JobType = request.JobType.Trim(),
                Location = request.Location.Trim(),
                SalaryPackage = request.SalaryPackage,
                MinimumCGPA = request.MinimumCGPA,
                MaximumBacklogs = request.MaximumBacklogs,
                ApplicationDeadline = request.ApplicationDeadline,
                IsOpenToAllColleges = request.IsOpenToAllColleges,
                Status = JobStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            // 5. Add Eligible Departments
            var departments = request.EligibleDepartments
                .Select(d => d.Trim())
                .Distinct()
                .Select(d => new JobEligibleDepartment
                {
                    JobId = job.JobId,
                    Department = d
                });
            await _jobRepository.AddEligibleDepartmentsAsync(departments);

            // 6. Add Eligible Graduation Years
            var years = request.EligibleGraduationYears
                .Distinct()
                .Select(y => new JobEligibleGraduationYear
                {
                    JobId = job.JobId,
                    GraduationYear = y
                });
            await _jobRepository.AddEligibleGraduationYearsAsync(years);

            // 7. Add Target Colleges if not open to all
            if (!request.IsOpenToAllColleges && request.TargetCollegeIds.Any())
            {
                var targetColleges = request.TargetCollegeIds
                    .Distinct()
                    .Select(cId => new JobTargetCollege
                    {
                        JobId = job.JobId,
                        CollegeId = cId
                    });
                await _jobRepository.AddTargetCollegesAsync(targetColleges);
            }

            await _jobRepository.SaveChangesAsync();

            return "Job opening created successfully as draft.";
        }

        // Update existing job opening
        public async Task<string> UpdateAsync(
            int jobId,
            UpdateJobOpeningDto request,
            string currentUserId,
            bool isSuperAdmin)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            // Resource check: HR can only edit their own company's jobs
            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to update job openings from another company.";
                }
            }

            // Deadline check
            if (request.ApplicationDeadline <= DateTime.UtcNow)
            {
                return "Application deadline must be a future date.";
            }

            // Validate target colleges when not open to all
            if (!request.IsOpenToAllColleges)
            {
                if (request.TargetCollegeIds == null || !request.TargetCollegeIds.Any())
                {
                    return "Please select at least one target college or enable open to all colleges.";
                }

                foreach (var collegeId in request.TargetCollegeIds.Distinct())
                {
                    var college = await _collegeRepository.GetByIdAsync(collegeId);
                    if (college == null || college.Status != CollegeStatus.Approved)
                    {
                        return $"Target college {collegeId} is not approved or does not exist.";
                    }
                }
            }

            // Update scalar fields
            job.JobTitle = request.JobTitle.Trim();
            job.JobDescription = request.JobDescription.Trim();
            job.JobType = request.JobType.Trim();
            job.Location = request.Location.Trim();
            job.SalaryPackage = request.SalaryPackage;
            job.MinimumCGPA = request.MinimumCGPA;
            job.MaximumBacklogs = request.MaximumBacklogs;
            job.ApplicationDeadline = request.ApplicationDeadline;
            job.IsOpenToAllColleges = request.IsOpenToAllColleges;

            _jobRepository.Update(job);

            // Refresh Eligible Departments
            var oldDepartments = await _jobRepository.GetEligibleDepartmentsByJobIdAsync(jobId);
            _jobRepository.RemoveEligibleDepartments(oldDepartments);

            var newDepartments = request.EligibleDepartments
                .Select(d => d.Trim())
                .Distinct()
                .Select(d => new JobEligibleDepartment
                {
                    JobId = jobId,
                    Department = d
                });
            await _jobRepository.AddEligibleDepartmentsAsync(newDepartments);

            // Refresh Eligible Graduation Years
            var oldYears = await _jobRepository.GetEligibleGraduationYearsByJobIdAsync(jobId);
            _jobRepository.RemoveEligibleGraduationYears(oldYears);

            var newYears = request.EligibleGraduationYears
                .Distinct()
                .Select(y => new JobEligibleGraduationYear
                {
                    JobId = jobId,
                    GraduationYear = y
                });
            await _jobRepository.AddEligibleGraduationYearsAsync(newYears);

            // Refresh Target Colleges
            var oldColleges = await _jobRepository.GetTargetCollegesByJobIdAsync(jobId);
            _jobRepository.RemoveTargetColleges(oldColleges);

            if (!request.IsOpenToAllColleges && request.TargetCollegeIds.Any())
            {
                var newColleges = request.TargetCollegeIds
                    .Distinct()
                    .Select(cId => new JobTargetCollege
                    {
                        JobId = jobId,
                        CollegeId = cId
                    });
                await _jobRepository.AddTargetCollegesAsync(newColleges);
            }

            await _jobRepository.SaveChangesAsync();

            return "Job opening updated successfully.";
        }

        // Publish job opening
        public async Task<string> PublishAsync(int jobId, string currentUserId, bool isSuperAdmin)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            // Resource authorization
            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to publish job openings from another company.";
                }
            }

            // Business rule: Only approved companies can publish jobs
            if (job.Company == null || job.Company.Status != CompanyStatus.Approved)
            {
                return "Only approved companies can publish job opportunities.";
            }

            if (job.ApplicationDeadline <= DateTime.UtcNow)
            {
                return "Cannot publish a job whose application deadline has already passed. Please update the deadline first.";
            }

            if (job.Status == JobStatus.Published)
            {
                return "Job opening is already published.";
            }

            job.Status = JobStatus.Published;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            return "Job opening published successfully.";
        }

        // Close job opening
        public async Task<string> CloseAsync(int jobId, string currentUserId, bool isSuperAdmin)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to close job openings from another company.";
                }
            }

            if (job.Status == JobStatus.Closed)
            {
                return "Job opening is already closed.";
            }

            job.Status = JobStatus.Closed;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            return "Job opening closed successfully.";
        }

        // Cancel job opening
        public async Task<string> CancelAsync(int jobId, string currentUserId, bool isSuperAdmin)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to cancel job openings from another company.";
                }
            }

            if (job.Status == JobStatus.Cancelled)
            {
                return "Job opening is already cancelled.";
            }

            job.Status = JobStatus.Cancelled;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            return "Job opening cancelled successfully.";
        }

        // Delete job opening safely
        public async Task<string> DeleteAsync(int jobId, string currentUserId, bool isSuperAdmin)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to delete job openings from another company.";
                }
            }

            try
            {
                // Remove child relationship records first
                var targetColleges = await _jobRepository.GetTargetCollegesByJobIdAsync(jobId);
                _jobRepository.RemoveTargetColleges(targetColleges);

                var departments = await _jobRepository.GetEligibleDepartmentsByJobIdAsync(jobId);
                _jobRepository.RemoveEligibleDepartments(departments);

                var years = await _jobRepository.GetEligibleGraduationYearsByJobIdAsync(jobId);
                _jobRepository.RemoveEligibleGraduationYears(years);

                _jobRepository.Delete(job);
                await _jobRepository.SaveChangesAsync();

                return "Job opening deleted successfully.";
            }
            catch (DbUpdateException)
            {
                return "Job opening cannot be deleted because related student applications exist. Close or cancel the job instead.";
            }
        }

        // Helper: Map entity to clean response DTO
        private async Task<JobOpeningResponseDto> MapToResponseDtoAsync(JobOpening job)
        {
            var targetColleges = await _jobRepository.GetTargetCollegesByJobIdAsync(job.JobId);
            var departments = await _jobRepository.GetEligibleDepartmentsByJobIdAsync(job.JobId);
            var years = await _jobRepository.GetEligibleGraduationYearsByJobIdAsync(job.JobId);

            return new JobOpeningResponseDto
            {
                JobId = job.JobId,
                CompanyId = job.CompanyId,
                CompanyName = job.Company?.CompanyName ?? string.Empty,
                CompanyEmail = job.Company?.CompanyEmail ?? string.Empty,
                CreatedByHRId = job.CreatedByHRId,
                CreatedByHRName = job.CreatedByHR?.User?.FullName ?? string.Empty,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobType = job.JobType,
                Location = job.Location,
                SalaryPackage = job.SalaryPackage,
                MinimumCGPA = job.MinimumCGPA,
                MaximumBacklogs = job.MaximumBacklogs,
                ApplicationDeadline = job.ApplicationDeadline,
                IsOpenToAllColleges = job.IsOpenToAllColleges,
                Status = job.Status,
                CreatedAt = job.CreatedAt,

                EligibleDepartments = departments.Select(d => d.Department).ToList(),
                EligibleGraduationYears = years.Select(y => y.GraduationYear).ToList(),
                TargetColleges = targetColleges.Select(tc => new TargetCollegeDto
                {
                    CollegeId = tc.CollegeId,
                    CollegeName = tc.College?.CollegeName ?? string.Empty,
                    CollegeCode = tc.College?.CollegeCode ?? string.Empty
                }).ToList()
            };
        }
    }
}
