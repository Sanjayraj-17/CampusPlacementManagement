using CampusPlacementManagement.API.DTOs.Interview;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Interfaces;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _interviewRepository;
        private readonly IJobOpeningRepository _jobOpeningRepository;
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly IStudentRepository _studentRepository;

        public InterviewService(
            IInterviewRepository interviewRepository,
            IJobOpeningRepository jobOpeningRepository,
            IJobApplicationRepository applicationRepository,
            ICompanyHRRepository companyHRRepository,
            IPlacementOfficerRepository officerRepository,
            IStudentRepository studentRepository)
        {
            _interviewRepository = interviewRepository;
            _jobOpeningRepository = jobOpeningRepository;
            _applicationRepository = applicationRepository;
            _companyHRRepository = companyHRRepository;
            _officerRepository = officerRepository;
            _studentRepository = studentRepository;
        }

        // ==========================================
        // INTERVIEW ROUND OPERATIONS
        // ==========================================

        public async Task<string> CreateRoundAsync(CreateInterviewRoundDto request, string currentUserId, bool isSuperAdmin)
        {
            var job = await _jobOpeningRepository.GetByIdAsync(request.JobId);
            if (job == null)
            {
                return "Job opening not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return "You are not authorized to configure interview rounds for another company's job opening.";
                }
            }

            var existingRound = await _interviewRepository.GetRoundByJobAndNumberAsync(job.JobId, request.RoundNumber);
            if (existingRound != null)
            {
                return $"Round number {request.RoundNumber} already exists for this job opening ('{existingRound.RoundName}').";
            }

            var round = new InterviewRound
            {
                JobId = request.JobId,
                RoundName = request.RoundName.Trim(),
                RoundNumber = request.RoundNumber,
                Description = request.Description?.Trim()
            };

            await _interviewRepository.AddRoundAsync(round);
            await _interviewRepository.SaveChangesAsync();

            return $"Interview round '{round.RoundName}' (Round {round.RoundNumber}) created successfully with ID: {round.InterviewRoundId}.";
        }

        public async Task<string> UpdateRoundAsync(int roundId, UpdateInterviewRoundDto request, string currentUserId, bool isSuperAdmin)
        {
            var round = await _interviewRepository.GetRoundByIdAsync(roundId);
            if (round == null)
            {
                return "Interview round not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != round.JobOpening.CompanyId)
                {
                    return "You are not authorized to update interview rounds for another company.";
                }
            }

            if (round.RoundNumber != request.RoundNumber)
            {
                var duplicate = await _interviewRepository.GetRoundByJobAndNumberAsync(round.JobId, request.RoundNumber);
                if (duplicate != null && duplicate.InterviewRoundId != roundId)
                {
                    return $"Round number {request.RoundNumber} already exists for this job opening ('{duplicate.RoundName}').";
                }
            }

            round.RoundName = request.RoundName.Trim();
            round.RoundNumber = request.RoundNumber;
            round.Description = request.Description?.Trim();

            _interviewRepository.UpdateRound(round);
            await _interviewRepository.SaveChangesAsync();

            return $"Interview round updated successfully.";
        }

        public async Task<string> DeleteRoundAsync(int roundId, string currentUserId, bool isSuperAdmin)
        {
            var round = await _interviewRepository.GetRoundByIdAsync(roundId);
            if (round == null)
            {
                return "Interview round not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != round.JobOpening.CompanyId)
                {
                    return "You are not authorized to delete interview rounds for another company.";
                }
            }

            _interviewRepository.DeleteRound(round);
            await _interviewRepository.SaveChangesAsync();

            return "Interview round deleted successfully.";
        }

        public async Task<InterviewRoundResponseDto?> GetRoundByIdAsync(int roundId)
        {
            var round = await _interviewRepository.GetRoundByIdAsync(roundId);
            if (round == null)
            {
                return null;
            }

            return MapToRoundDto(round);
        }

        public async Task<List<InterviewRoundResponseDto>> GetRoundsByJobIdAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var job = await _jobOpeningRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return new List<InterviewRoundResponseDto>();
            }

            // Boundary verification
            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return new List<InterviewRoundResponseDto>();
                }
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<InterviewRoundResponseDto>();
                }

                var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(jobId);
                var isVisible = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == officer.CollegeId);
                if (!isVisible)
                {
                    return new List<InterviewRoundResponseDto>();
                }
            }
            else if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null)
                {
                    return new List<InterviewRoundResponseDto>();
                }

                var targetColleges = await _jobOpeningRepository.GetTargetCollegesByJobIdAsync(jobId);
                var isVisible = job.IsOpenToAllColleges || targetColleges.Any(tc => tc.CollegeId == student.CollegeId);
                if (!isVisible)
                {
                    return new List<InterviewRoundResponseDto>();
                }
            }

            var rounds = await _interviewRepository.GetRoundsByJobIdAsync(jobId);
            return rounds.Select(MapToRoundDto).ToList();
        }

        // ==========================================
        // INTERVIEW SCHEDULE OPERATIONS
        // ==========================================

        public async Task<string> ScheduleInterviewAsync(ScheduleInterviewRequestDto request, string currentUserId, bool isSuperAdmin)
        {
            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId);
            if (application == null)
            {
                return "Job application not found.";
            }

            if (application.Status == ApplicationStatus.Withdrawn)
            {
                return "Cannot schedule an interview for a withdrawn application.";
            }

            if (application.Status == ApplicationStatus.Rejected)
            {
                return "Cannot schedule an interview for a rejected application.";
            }

            var round = await _interviewRepository.GetRoundByIdAsync(request.InterviewRoundId);
            if (round == null)
            {
                return "Interview round not found.";
            }

            if (round.JobId != application.JobId)
            {
                return "The specified interview round does not belong to the job opening of this application.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != application.JobOpening.CompanyId)
                {
                    return "You are not authorized to schedule interviews for another company's candidates.";
                }
            }

            if (request.ScheduledAt < DateTime.UtcNow.AddMinutes(-10))
            {
                return "Scheduled interview date and time cannot be in the past.";
            }

            var schedule = new InterviewSchedule
            {
                ApplicationId = request.ApplicationId,
                InterviewRoundId = request.InterviewRoundId,
                ScheduledAt = request.ScheduledAt,
                MeetingLink = request.MeetingLink?.Trim(),
                Venue = request.Venue?.Trim(),
                Status = InterviewScheduleStatus.Scheduled
            };

            await _interviewRepository.AddScheduleAsync(schedule);

            // Update Application status if not already InterviewScheduled
            if (application.Status != ApplicationStatus.InterviewScheduled)
            {
                var prevStatus = application.Status;
                application.Status = ApplicationStatus.InterviewScheduled;
                application.LastUpdatedAt = DateTime.UtcNow;

                _applicationRepository.Update(application);

                var history = new ApplicationStatusHistory
                {
                    ApplicationId = application.ApplicationId,
                    PreviousStatus = prevStatus,
                    NewStatus = ApplicationStatus.InterviewScheduled,
                    UpdatedByUserId = currentUserId,
                    Remarks = $"Interview scheduled for {round.RoundName} (Round {round.RoundNumber}).",
                    UpdatedAt = DateTime.UtcNow
                };

                await _applicationRepository.AddStatusHistoryAsync(history);
            }

            await _interviewRepository.SaveChangesAsync();

            return $"Interview scheduled successfully with Schedule ID: {schedule.InterviewScheduleId}.";
        }

        public async Task<string> UpdateScheduleAsync(int scheduleId, UpdateInterviewScheduleDto request, string currentUserId, bool isSuperAdmin)
        {
            var schedule = await _interviewRepository.GetScheduleByIdAsync(scheduleId);
            if (schedule == null)
            {
                return "Interview schedule not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != schedule.JobApplication.JobOpening.CompanyId)
                {
                    return "You are not authorized to update interview schedules for another company.";
                }
            }

            schedule.ScheduledAt = request.ScheduledAt;
            schedule.MeetingLink = request.MeetingLink?.Trim();
            schedule.Venue = request.Venue?.Trim();
            schedule.Status = request.Status;
            schedule.Feedback = request.Feedback?.Trim();

            _interviewRepository.UpdateSchedule(schedule);
            await _interviewRepository.SaveChangesAsync();

            return $"Interview schedule updated successfully (Status: {schedule.Status}).";
        }

        public async Task<string> DeleteScheduleAsync(int scheduleId, string currentUserId, bool isSuperAdmin)
        {
            var schedule = await _interviewRepository.GetScheduleByIdAsync(scheduleId);
            if (schedule == null)
            {
                return "Interview schedule not found.";
            }

            if (!isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != schedule.JobApplication.JobOpening.CompanyId)
                {
                    return "You are not authorized to delete interview schedules for another company.";
                }
            }

            _interviewRepository.DeleteSchedule(schedule);
            await _interviewRepository.SaveChangesAsync();

            return "Interview schedule deleted successfully.";
        }

        public async Task<InterviewScheduleResponseDto?> GetScheduleByIdAsync(
            int scheduleId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            bool isStudent)
        {
            var schedule = await _interviewRepository.GetScheduleByIdAsync(scheduleId);
            if (schedule == null)
            {
                return null;
            }

            if (isStudent && !isSuperAdmin)
            {
                var student = await _studentRepository.GetByUserIdAsync(currentUserId);
                if (student == null || schedule.JobApplication.StudentId != student.StudentId)
                {
                    return null;
                }
            }
            else if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != schedule.JobApplication.JobOpening.CompanyId)
                {
                    return null;
                }
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null || officer.CollegeId != schedule.JobApplication.Student.CollegeId)
                {
                    return null;
                }
            }

            return MapToScheduleDto(schedule);
        }

        public async Task<List<InterviewScheduleResponseDto>> GetMySchedulesAsync(string currentUserId)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return new List<InterviewScheduleResponseDto>();
            }

            var schedules = await _interviewRepository.GetSchedulesByStudentIdAsync(student.StudentId);
            return schedules.Select(MapToScheduleDto).ToList();
        }

        public async Task<List<InterviewScheduleResponseDto>> GetSchedulesForJobAsync(
            int jobId,
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            InterviewScheduleStatus? status = null)
        {
            var job = await _jobOpeningRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return new List<InterviewScheduleResponseDto>();
            }

            List<InterviewSchedule> schedules;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null || hr.CompanyId != job.CompanyId)
                {
                    return new List<InterviewScheduleResponseDto>();
                }

                schedules = await _interviewRepository.GetSchedulesByJobIdAsync(jobId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<InterviewScheduleResponseDto>();
                }

                var allSchedules = await _interviewRepository.GetSchedulesByJobIdAsync(jobId, status);
                schedules = allSchedules.Where(s => s.JobApplication.Student.CollegeId == officer.CollegeId).ToList();
            }
            else if (isSuperAdmin)
            {
                schedules = await _interviewRepository.GetSchedulesByJobIdAsync(jobId, status);
            }
            else
            {
                return new List<InterviewScheduleResponseDto>();
            }

            return schedules.Select(MapToScheduleDto).ToList();
        }

        public async Task<List<InterviewScheduleResponseDto>> GetAllSchedulesAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isCompanyHR,
            bool isPlacementOfficer,
            InterviewScheduleStatus? status = null)
        {
            List<InterviewSchedule> schedules;

            if (isCompanyHR && !isSuperAdmin)
            {
                var hr = await _companyHRRepository.GetByUserIdAsync(currentUserId);
                if (hr == null)
                {
                    return new List<InterviewScheduleResponseDto>();
                }

                schedules = await _interviewRepository.GetSchedulesByCompanyIdAsync(hr.CompanyId, status);
            }
            else if (isPlacementOfficer && !isSuperAdmin)
            {
                var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
                if (officer == null)
                {
                    return new List<InterviewScheduleResponseDto>();
                }

                schedules = await _interviewRepository.GetSchedulesByCollegeIdAsync(officer.CollegeId, status);
            }
            else if (isSuperAdmin)
            {
                schedules = await _interviewRepository.GetAllSchedulesAsync(status);
            }
            else
            {
                return new List<InterviewScheduleResponseDto>();
            }

            return schedules.Select(MapToScheduleDto).ToList();
        }

        private static InterviewRoundResponseDto MapToRoundDto(InterviewRound round)
        {
            return new InterviewRoundResponseDto
            {
                InterviewRoundId = round.InterviewRoundId,
                JobId = round.JobId,
                JobTitle = round.JobOpening?.JobTitle ?? string.Empty,
                RoundName = round.RoundName,
                RoundNumber = round.RoundNumber,
                Description = round.Description
            };
        }

        private static InterviewScheduleResponseDto MapToScheduleDto(InterviewSchedule schedule)
        {
            return new InterviewScheduleResponseDto
            {
                InterviewScheduleId = schedule.InterviewScheduleId,
                ApplicationId = schedule.ApplicationId,
                JobId = schedule.JobApplication.JobId,
                JobTitle = schedule.JobApplication.JobOpening?.JobTitle ?? string.Empty,
                CompanyId = schedule.JobApplication.JobOpening?.CompanyId ?? 0,
                CompanyName = schedule.JobApplication.JobOpening?.Company?.CompanyName ?? string.Empty,
                StudentId = schedule.JobApplication.StudentId,
                StudentFullName = schedule.JobApplication.Student?.User?.FullName ?? string.Empty,
                StudentEmail = schedule.JobApplication.Student?.User?.Email ?? string.Empty,
                RegisterNumber = schedule.JobApplication.Student?.RegisterNumber ?? string.Empty,
                Department = schedule.JobApplication.Student?.Department ?? string.Empty,
                CollegeId = schedule.JobApplication.Student?.CollegeId ?? 0,
                CollegeName = schedule.JobApplication.Student?.College?.CollegeName ?? string.Empty,
                InterviewRoundId = schedule.InterviewRoundId,
                RoundName = schedule.InterviewRound?.RoundName ?? string.Empty,
                RoundNumber = schedule.InterviewRound?.RoundNumber ?? 0,
                ScheduledAt = schedule.ScheduledAt,
                MeetingLink = schedule.MeetingLink,
                Venue = schedule.Venue,
                Status = schedule.Status,
                Feedback = schedule.Feedback
            };
        }
    }
}
