using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class JobOpening
    {
        public int JobId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int CreatedByHRId { get; set; }

        [Required]
        [MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal? SalaryPackage { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? MinimumCGPA { get; set; }

        public int? MaximumBacklogs { get; set; }

        [Required]
        public DateTime ApplicationDeadline { get; set; }

        [Required]
        public bool IsOpenToAllColleges { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        [ForeignKey(nameof(CreatedByHRId))]
        public CompanyHR CreatedByHR { get; set; } = null!;
    }
}