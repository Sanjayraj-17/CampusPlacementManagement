using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class ApplicationStatusHistory
    {
        public int HistoryId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        public ApplicationStatus? PreviousStatus { get; set; }

        [Required]
        public ApplicationStatus NewStatus { get; set; }

        [Required]
        public string UpdatedByUserId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        [ForeignKey(nameof(ApplicationId))]
        public JobApplication JobApplication { get; set; } = null!;

        [ForeignKey(nameof(UpdatedByUserId))]
        public ApplicationUser UpdatedByUser { get; set; } = null!;
    }
}