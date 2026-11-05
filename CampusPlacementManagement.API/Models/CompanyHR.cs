
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementManagement.API.Models
{
    public class CompanyHR
    {
        public int CompanyHRId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        [ForeignKey(nameof(ApprovedBy))]
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
