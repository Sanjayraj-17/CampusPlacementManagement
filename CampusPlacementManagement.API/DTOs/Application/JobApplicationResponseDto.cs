using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Application
{
    // Complete DTO returned when retrieving job application details
    public class JobApplicationResponseDto
    {
        public int ApplicationId { get; set; }

        // Job Details
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal? SalaryPackage { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        // Company Details
        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        // Student Details
        public int StudentId { get; set; }

        public string StudentUserId { get; set; } = string.Empty;

        public string StudentFullName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public string RegisterNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;

        public int GraduationYear { get; set; }

        public decimal CGPA { get; set; }

        public int ActiveBacklogs { get; set; }

        // College Details
        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        // Application Status & Metadata
        public ApplicationStatus Status { get; set; }

        public string? ResumeUrl { get; set; }

        public DateTime AppliedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        // Audit Trail of Status Changes
        public List<ApplicationStatusHistoryDto> StatusHistory { get; set; }
            = new List<ApplicationStatusHistoryDto>();
    }
}
