using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Job
{
    public class JobOpeningResponseDto
    {
        public int JobId { get; set; }

        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        public int CreatedByHRId { get; set; }

        public string CreatedByHRName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal? SalaryPackage { get; set; }

        public decimal? MinimumCGPA { get; set; }

        public int? MaximumBacklogs { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public bool IsOpenToAllColleges { get; set; }

        public JobStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> EligibleDepartments { get; set; } = new List<string>();

        public List<int> EligibleGraduationYears { get; set; } = new List<int>();

        public List<TargetCollegeDto> TargetColleges { get; set; } = new List<TargetCollegeDto>();
    }

    public class TargetCollegeDto
    {
        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;
    }
}
