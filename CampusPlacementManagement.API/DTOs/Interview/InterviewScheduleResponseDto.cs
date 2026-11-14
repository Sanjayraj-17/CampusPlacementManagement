using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Interview
{
    // DTO returned when retrieving interview schedule details
    public class InterviewScheduleResponseDto
    {
        public int InterviewScheduleId { get; set; }

        public int ApplicationId { get; set; }

        // Job & Company Details
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        // Student Details
        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public string RegisterNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        // Interview Round Details
        public int InterviewRoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public int RoundNumber { get; set; }

        // Schedule Specifics
        public DateTime ScheduledAt { get; set; }

        public string? MeetingLink { get; set; }

        public string? Venue { get; set; }

        public InterviewScheduleStatus Status { get; set; }

        public string? Feedback { get; set; }
    }
}
