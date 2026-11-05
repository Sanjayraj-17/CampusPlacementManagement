
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CollegeId { get; set; }

        [Required]
        public int PlacementOfficerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RegisterNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required]
        public int GraduationYear { get; set; }

        [Required]
        [Column(TypeName = "decimal(4,2)")]
        public decimal CGPA { get; set; }

        [Required]
        public int ActiveBacklogs { get; set; }

        [MaxLength(500)]
        public string? ResumeUrl { get; set; }

        [Required]
        public AccountStatus Status { get; set; }
            = AccountStatus.Pending;

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(CollegeId))]
        public College College { get; set; } = null!;

        [ForeignKey(nameof(PlacementOfficerId))]
        public PlacementOfficer PlacementOfficer { get; set; } = null!;
    }
}
