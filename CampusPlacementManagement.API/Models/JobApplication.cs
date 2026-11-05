using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class JobApplication
    {
        public int ApplicationId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public ApplicationStatus Status { get; set; }
            = ApplicationStatus.Applied;

        [MaxLength(500)]
        public string? ResumeUrl { get; set; }

        [Required]
        public DateTime LastUpdatedAt { get; set; }
            = DateTime.UtcNow;

        // Navigation Properties

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; } = null!;
    }
}