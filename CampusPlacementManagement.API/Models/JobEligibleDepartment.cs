using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementManagement.API.Models
{
    public class JobEligibleDepartment
    {
        public int JobEligibleDepartmentId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        // Navigation Property

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;
    }
}