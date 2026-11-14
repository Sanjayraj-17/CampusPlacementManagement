using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Job
{
    public class CreateJobOpeningDto
    {
        [Required(ErrorMessage = "Job title is required.")]
        [MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        public string JobDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job type is required.")]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 99999999.99, ErrorMessage = "Salary package must be a positive value.")]
        public decimal? SalaryPackage { get; set; }

        [Range(0.00, 10.00, ErrorMessage = "Minimum CGPA must be between 0.00 and 10.00.")]
        public decimal? MinimumCGPA { get; set; }

        [Range(0, 50, ErrorMessage = "Maximum backlogs cannot be negative.")]
        public int? MaximumBacklogs { get; set; }

        [Required(ErrorMessage = "Application deadline is required.")]
        public DateTime ApplicationDeadline { get; set; }

        public bool IsOpenToAllColleges { get; set; } = false;

        // Specific colleges to target when IsOpenToAllColleges is false
        public List<int> TargetCollegeIds { get; set; } = new List<int>();

        // Eligible departments (e.g. "Computer Science and Engineering", "Information Technology")
        [Required(ErrorMessage = "At least one eligible department is required.")]
        [MinLength(1, ErrorMessage = "At least one eligible department is required.")]
        public List<string> EligibleDepartments { get; set; } = new List<string>();

        // Eligible graduation years (e.g. 2026, 2027)
        [Required(ErrorMessage = "At least one eligible graduation year is required.")]
        [MinLength(1, ErrorMessage = "At least one eligible graduation year is required.")]
        public List<int> EligibleGraduationYears { get; set; } = new List<int>();

        // Optional when created by Company HR (inferred from HR profile);
        // Required when created by SuperAdmin.
        public int? CompanyId { get; set; }

        public int? CreatedByHRId { get; set; }
    }
}
