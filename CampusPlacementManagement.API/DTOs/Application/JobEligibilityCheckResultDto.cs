namespace CampusPlacementManagement.API.DTOs.Application
{
    // DTO returned when a student verifies their eligibility for a job opening
    public class JobEligibilityCheckResultDto
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public bool IsEligible { get; set; }

        public bool HasAlreadyApplied { get; set; }

        public int? ExistingApplicationId { get; set; }

        public bool MeetsCGPA { get; set; }

        public decimal StudentCGPA { get; set; }

        public decimal? MinimumCGPARequired { get; set; }

        public bool MeetsBacklogs { get; set; }

        public int StudentActiveBacklogs { get; set; }

        public int? MaximumBacklogsAllowed { get; set; }

        public bool MeetsDepartment { get; set; }

        public string StudentDepartment { get; set; } = string.Empty;

        public List<string> EligibleDepartments { get; set; } = new List<string>();

        public bool MeetsGraduationYear { get; set; }

        public int StudentGraduationYear { get; set; }

        public List<int> EligibleGraduationYears { get; set; } = new List<int>();

        public bool MeetsCollegeTargeting { get; set; }

        public bool IsJobOpen { get; set; }

        public List<string> IneligibilityReasons { get; set; } = new List<string>();
    }
}
