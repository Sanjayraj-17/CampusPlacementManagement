
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class PlacementOfficer
    {
        public int PlacementOfficerId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CollegeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public AccountStatus Status { get; set; }
            = AccountStatus.Pending;

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(CollegeId))]
        public College College { get; set; } = null!;

        [ForeignKey(nameof(ApprovedBy))]
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
