using CampusPlacementManagement.API.DTOs.Application;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly IJobOpeningRepository _jobOpeningRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly ICollegeRepository _collegeRepository;

        public JobApplicationService(
            IJobApplicationRepository applicationRepository,
            IJobOpeningRepository jobOpeningRepository,
            IStudentRepository studentRepository,
            ICompanyHRRepository companyHRRepository,
            IPlacementOfficerRepository officerRepository,
            ICollegeRepository collegeRepository)
        {
            _applicationRepository = applicationRepository;
            _jobOpeningRepository = jobOpeningRepository;
            _studentRepository = studentRepository;
            _companyHRRepository = companyHRRepository;
            _officerRepository = officerRepository;
            _collegeRepository = collegeRepository;
        }

        // Student applies for a job opening with full eligibility verification
        public async Task<string> ApplyAsync(ApplyJobRequestDto request, string currentUserId)
        {
            // 1. Verify Student profile
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return "Student profile not found.";
            }

            if (student.Status != AccountStatus.Approved)
            {
                return "Your student account is not approved yet. Only approved students can apply for jobs.";
            }

            // 2. Verify Student's College is Approved
            var college = await _collegeRepository.GetByIdAsync(student.CollegeId);
            if (college == null || college.Status != CollegeStatus.Approved)
            {
                return "Your college is not currently approved for placement activities.";
            }

            // 3. Determine Resume URL to use
            var resumeToUse = !string.IsNullOrWhiteSpace(request.ResumeUrl)
                ? request.ResumeUrl.Trim()
                : student.ResumeUrl;

            if (string.IsNullOrWhiteSpace(resumeToUse))
            {
                return "Please provide a resume URL in the application or update your profile resume first.";
            }

            // 4. Verify Job Opening exists and is active
            var job = await _jobOpeningRepository.GetByIdAsync(request.JobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            if (job.Status != JobStatus.Published)
            {
                return $"This job opening is not currently accepting applications (Status: {job.Status}).";
            }

            if (DateTime.UtcNow > job.ApplicationDeadline)
            {
                return $"The application deadline for this job has passed ({job.ApplicationDeadline:yyyy-MM-dd}).";
            }

            // 5. Prevent Duplicate Applications
            var existingApplication = await _applicationRepository.GetByStudentAndJobAsync(student.StudentId, job.JobId);
            if (existingApplication != null)
            {
                return $"You have already applied for this job on {existingApplication.AppliedAt:yyyy-MM-dd} (Current Status: {existingApplication.Status}).";
            }

            // 6. Check College Targeting
            if (!job.IsOpenToAllColleges)
            {
                var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(job.JobId);
                var isTargeted = targetColleges.Any(tc => tc.CollegeId == student.CollegeId);
                if (!isTargeted)
                {
                    return "This job opening is not open to students from your college.";
                }
            }

            // 7. Check Academic Eligibility (CGPA, Backlogs, Department, Graduation Year)
            if (job.MinimumCGPA.HasValue && student.CGPA < job.MinimumCGPA.Value)
            {
                return $"Your CGPA ({student.CGPA:F2}) does not meet the minimum requirement of {job.MinimumCGPA.Value:F2}.";
            }

            if (job.MaximumBacklogs.HasValue && student.ActiveBacklogs > job.MaximumBacklogs.Value)
            {
                return $"Your active backlogs ({student.ActiveBacklogs}) exceed the maximum allowed limit of {job.MaximumBacklogs.Value}.";
            }

            var eligibleDepartments = await _jobOpeningRepository.GetEligibleDepartmentsByJobIdAsync(job.JobId);
            if (eligibleDepartments.Any())
            {
                var isDeptEligible = eligibleDepartments.Any(d =>
                    d.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase));

                if (!isDeptEligible)
                {
                    return $"Your department ({student.Department}) is not eligible for this job opening.";
                }
            }

            var eligibleGraduationYears = await _jobOpeningRepository.GetEligibleGraduationYearsByJobIdAsync(job.JobId);
            if (eligibleGraduationYears.Any())
            {
                var isYearEligible = eligibleGraduationYears.Any(y => y.GraduationYear == student.GraduationYear);
                if (!isYearEligible)
                {
                    return $"Your graduation year ({student.GraduationYear}) is not eligible for this job opening.";
                }
            }

            // 8. Create Job Application
            var application = new JobApplication
            {
                JobId = job.JobId,
                StudentId = student.StudentId,
                ResumeUrl = resumeToUse,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // 9. Record initial status in ApplicationStatusHistory
            var initialHistory = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = null,
                NewStatus = ApplicationStatus.Applied,
                UpdatedByUserId = currentUserId,
                Remarks = "Application submitted by student.",
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(initialHistory);
            await _applicationRepository.SaveChangesAsync();

            return $"Job application submitted successfully with Application ID: {application.ApplicationId}.";
        }

        // Student checks their eligibility for a job opening
        public async Task<JobEligibilityCheckResultDto?> CheckEligibilityAsync(int jobId, string currentUserId)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return null;
            }

            var job = await _jobOpeningRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return null;
            }

            var existingApp = await _applicationRepository.GetByStudentAndJobAsync(student.StudentId, job.JobId);
            var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(job.JobId);
            var eligibleDepts = await _jobOpeningRepository.GetEligibleDepartmentsByJobIdAsync(job.JobId);
            var eligibleYears = await _jobOpeningRepository.GetEligibleGraduationYearsByJobIdAsync(job.JobId);

            var reasons = new List<string>();

            // Check Account & College status
            if (student.Status != AccountStatus.Approved)
            {
                reasons.Add("Your student account is not approved yet.");
            }

            var college = await _collegeRepository.GetByIdAsync(student.CollegeId);
            if (college == null || college.Status != CollegeStatus.Approved)
            {
                reasons.Add("Your college is not currently approved for placement activities.");
            }

            // Check Job Status & Deadline
            var isJobOpen = job.Status == JobStatus.Published && DateTime.UtcNow <= job.ApplicationDeadline;
            if (job.Status != JobStatus.Published)
            {
                reasons.Add($"Job is not accepting applications (Current Status: {job.Status}).");
            }
            if (DateTime.UtcNow > job.ApplicationDeadline)
            {
                reasons.Add($"Job application deadline has passed ({job.ApplicationDeadline:yyyy-MM-dd}).");
            }

            // Check College Targeting
            var meetsCollege = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == student.CollegeId);
            if (!meetsCollege)
            {
                reasons.Add("Job opening is not open to students from your college.");
            }

            // Check CGPA
            var meetsCGPA = !job.MinimumCGPA.HasValue || student.CGPA >= job.MinimumCGPA.Value;
            if (!meetsCGPA)
            {
                reasons.Add($"Your CGPA ({student.CGPA:F2}) is below required {job.MinimumCGPA!.Value:F2}.");
            }

            // Check Backlogs
            var meetsBacklogs = !job.MaximumBacklogs.HasValue || student.ActiveBacklogs <= job.MaximumBacklogs.Value;
            if (!meetsBacklogs)
            {
                reasons.Add($"Your active backlogs ({student.ActiveBacklogs}) exceed maximum allowed {job.MaximumBacklogs!.Value}.");
            }

            // Check Department
            var deptList = eligibleDepts.Select(d => d.Department).ToList();
            var meetsDept = !deptList.Any() || deptList.Any(d => d.Equals(student.Department, StringComparison.OrdinalIgnoreCase));
            if (!meetsDept)
            {
                reasons.Add($"Your department ({student.Department}) is not in the eligible departments list.");
            }

            // Check Graduation Year
            var yearList = eligibleYears.Select(y => y.GraduationYear).ToList();
            var meetsYear = !yearList.Any() || yearList.Contains(student.GraduationYear);
            if (!meetsYear)
            {
                reasons.Add($"Your graduation year ({student.GraduationYear}) is not in the eligible graduation years list.");
            }

            var hasAlreadyApplied = existingApp != null;
            if (hasAlreadyApplied)
            {
                reasons.Add($"You have already applied for this job on {existingApp!.AppliedAt:yyyy-MM-dd}.");
            }

            var isEligible = reasons.Count == 0 && !hasAlreadyApplied;

            return new JobEligibilityCheckResultDto
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                IsEligible = isEligible,
                HasAlreadyApplied = hasAlreadyApplied,
                ExistingApplicationId = existingApp?.ApplicationId,
                MeetsCGPA = meetsCGPA,
                StudentCGPA = student.CGPA,
                MinimumCGPARequired = job.MinimumCGPA,
                MeetsBacklogs = meetsBacklogs,
                StudentActiveBacklogs = student.ActiveBacklogs,
                MaximumBacklogsAllowed = job.MaximumBacklogs,
                MeetsDepartment = meetsDept,
                StudentDepartment = student.Department,
                EligibleDepartments = deptList,
                MeetsGraduationYear = meetsYear,
                StudentGraduationYear = student.GraduationYear,
                EligibleGraduationYears = yearList,
                MeetsCollegeTargeting = meetsCollege,
                IsJobOpen = isJobOpen,
                IneligibilityReasons = reasons
            };
        }

        // Student withdraws their job application
        public async Task<string> WithdrawAsync(int applicationId, string currentUserId)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return "Student profile not found.";
            }

            var app = await _applicationRepository.GetByIdAsync(applicationId);
            if (app == null)
            {
                return "Job application not found.";
            }

            if (app.StudentId != student.StudentId)
            {
                return "You are not authorized to withdraw another student's application.";
            }

            if (app.Status == ApplicationStatus.Withdrawn)
            {
                return "This application has already been withdrawn.";
            }

            if (app.Status == ApplicationStatus.Selected)
            {
                return "Cannot withdraw an application that has already been selected for hire.";
            }

            if (app.Status == ApplicationStatus.Rejected)
            {
                return "Cannot withdraw an application that has already been rejected.";
            }

            var prevStatus = app.Status;
            app.Status = ApplicationStatus.Withdrawn;
            app.LastUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(app);

            // Record audit trail
            var history = new ApplicationStatusHistory
            {
                ApplicationId = app.ApplicationId,
                PreviousStatus = prevStatus,
                NewStatus = ApplicationStatus.Withdrawn,
                UpdatedByUserId = currentUserId,
                Remarks = "Application withdrawn by student.",
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(history);
            await _applicationRepository.SaveChangesAsync();

            return "Job application withdrawn successfully.";
        }

        // Company HR or SuperAdmin updates application status
        public async Task<string> UpdateStatusAsync(int applicationId, UpdateApplicationStatusDto request, string currentUserId, bool isSuperAdmin)
        {
            var app = await _applicationRepository.GetByIdAsync(applicationId);
            if (app == null)
            {
                return "Job application not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != app.JobOpening.CompanyId)
                {
                    return "You are not authorized to update applications for another company's job opening.";
                }
            }

            if (app.Status == ApplicationStatus.Withdrawn)
            {
                return "Cannot update the status of an application that was withdrawn by the student.";
            }

            if (app.Status == request.NewStatus)
            {
                return $"The application is already in '{request.NewStatus}' status.";
            }

            var prevStatus = app.Status;
            app.Status = request.NewStatus;
            app.LastUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(app);

            // Record audit trail
            var history = new ApplicationStatusHistory
            {
                ApplicationId = app.ApplicationId,
                PreviousStatus = prevStatus,
                NewStatus = request.NewStatus,
                UpdatedByUserId = currentUserId,
                Remarks = !string.IsNullOrWhiteSpace(request.Remarks)
                    ? request.Remarks.Trim()
                    : $"Status updated from {prevStatus} to {request.NewStatus}.",
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(history);
            await _applicationRepository.SaveChangesAsync();

            return $"Job application status updated successfully to '{request.NewStatus}'.";
        }

        // View single application details (resource-authorized)
        public async Task<JobApplicationResponseDto?> GetByIdAsync(
            int applicationId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var app = await _applicationRepository.GetByIdAsync(applicationId);
            if (app == null)
            {
                return null;
            }

            // Resource-based authorization
            if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null || app.StudentId != student.StudentId)
                {
                    return null;
                }
            }
            else if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || app.JobOpening.CompanyId != hr.CompanyId)
                {
                    return null;
                }
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || app.Student.CollegeId != officer.CollegeId)
                {
                    return null;
                }
            }

            return await MapToResponseDtoAsync(app);
        }

        // Student views all their own job applications
        public async Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(string currentUserId)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return new List<JobApplicationResponseDto>();
            }

            var applications = await _applicationRepository.GetByStudentIdAsync(student.StudentId);
            var results = new List<JobApplicationResponseDto>();

            foreach (var app in applications)
            {
                results.Add(await MapToResponseDtoAsync(app));
            }

            return results;
        }

        // View applications for a specific job
        public async Task<List<JobApplicationResponseDto>> GetApplicationsForJobAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            ApplicationStatus? status = null)
        {
            var job = await _jobOpeningRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return new List<JobApplicationResponseDto>();
            }

            List<JobApplication> applications;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return new List<JobApplicationResponseDto>();
                }

                applications = await _applicationRepository.GetByJobIdAsync(jobId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<JobApplicationResponseDto>();
                }

                // Verify this job targets this officer's college
                var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(jobId);
                var isVisible = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == officer.CollegeId);
                if (!isVisible)
                {
                    return new List<JobApplicationResponseDto>();
                }

                // Placement officer only sees applications from their own college's students
                var allApps = await _applicationRepository.GetByJobIdAsync(jobId, status);
                applications = allApps.Where(a => a.Student.CollegeId == officer.CollegeId).ToList();
            }
            else if (isSuperAdmin)
            {
                applications = await _applicationRepository.GetByJobIdAsync(jobId, status);
            }
            else
            {
                return new List<JobApplicationResponseDto>();
            }

            var results = new List<JobApplicationResponseDto>();
            foreach (var app in applications)
            {
                results.Add(await MapToResponseDtoAsync(app));
            }

            return results;
        }

        // View all applications accessible by role
        public async Task<List<JobApplicationResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            ApplicationStatus? status = null)
        {
            List<JobApplication> applications;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return new List<JobApplicationResponseDto>();
                }

                applications = await _applicationRepository.GetByCompanyIdAsync(hr.CompanyId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<JobApplicationResponseDto>();
                }

                applications = await _applicationRepository.GetByCollegeIdAsync(officer.CollegeId, status);
            }
            else if (isSuperAdmin)
            {
                applications = await _applicationRepository.GetAllAsync(status);
            }
            else
            {
                return new List<JobApplicationResponseDto>();
            }

            var results = new List<JobApplicationResponseDto>();
            foreach (var app in applications)
            {
                results.Add(await MapToResponseDtoAsync(app));
            }

            return results;
        }

        // Helper method to map JobApplication entity and its audit history to JobApplicationResponseDto
        private async Task<JobApplicationResponseDto> MapToResponseDtoAsync(JobApplication app)
        {
            var histories = await _applicationRepository.GetStatusHistoryByApplicationIdAsync(app.ApplicationId);

            var historyDtos = histories.Select(h => new ApplicationStatusHistoryDto
            {
                HistoryId = h.HistoryId,
                ApplicationId = h.ApplicationId,
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                UpdatedByUserId = h.UpdatedByUserId,
                UpdatedByName = h.UpdatedByUser?.FullName ?? h.UpdatedByUserId,
                Remarks = h.Remarks,
                UpdatedAt = h.UpdatedAt
            }).ToList();

            return new JobApplicationResponseDto
            {
                ApplicationId = app.ApplicationId,
                JobId = app.JobId,
                JobTitle = app.JobOpening.JobTitle,
                JobType = app.JobOpening.JobType,
                Location = app.JobOpening.Location,
                SalaryPackage = app.JobOpening.SalaryPackage,
                ApplicationDeadline = app.JobOpening.ApplicationDeadline,
                CompanyId = app.JobOpening.CompanyId,
                CompanyName = app.JobOpening.Company.CompanyName,
                CompanyEmail = app.JobOpening.Company.CompanyEmail,
                StudentId = app.StudentId,
                StudentUserId = app.Student.UserId,
                StudentFullName = app.Student.User?.FullName ?? string.Empty,
                StudentEmail = app.Student.User?.Email ?? string.Empty,
                RegisterNumber = app.Student.RegisterNumber,
                Department = app.Student.Department,
                Degree = app.Student.Degree,
                GraduationYear = app.Student.GraduationYear,
                CGPA = app.Student.CGPA,
                ActiveBacklogs = app.Student.ActiveBacklogs,
                CollegeId = app.Student.CollegeId,
                CollegeName = app.Student.College?.CollegeName ?? string.Empty,
                CollegeCode = app.Student.College?.CollegeCode ?? string.Empty,
                Status = app.Status,
                ResumeUrl = app.ResumeUrl,
                AppliedAt = app.AppliedAt,
                LastUpdatedAt = app.LastUpdatedAt,
                StatusHistory = historyDtos
            };
        }
    }
}
