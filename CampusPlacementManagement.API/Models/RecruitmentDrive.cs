using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class RecruitmentDrive
    {
        public int DriveId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int CollegeId { get; set; }

        [Required]
        public DateTime DriveDate { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }

        public string? Description { get; set; }

        [Required]
        public RecruitmentDriveStatus Status { get; set; }
            = RecruitmentDriveStatus.Scheduled;

        // Navigation Properties

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;

        [ForeignKey(nameof(CollegeId))]
        public College College { get; set; } = null!;
    }
}