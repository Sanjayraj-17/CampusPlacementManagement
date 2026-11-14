using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.RecruitmentDrive
{
    // DTO returned when retrieving recruitment drive information
    public class RecruitmentDriveResponseDto
    {
        public int DriveId { get; set; }

        // Job Details
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string JobLocation { get; set; } = string.Empty;

        // Company Details
        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        // College Details
        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        // Drive Details
        public DateTime DriveDate { get; set; }

        public string? Venue { get; set; }

        public string? Description { get; set; }

        public RecruitmentDriveStatus Status { get; set; }
    }
}
