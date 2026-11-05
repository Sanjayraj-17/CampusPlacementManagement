using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementManagement.API.Models
{
    public class JobTargetCollege
    {
        public int JobTargetCollegeId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int CollegeId { get; set; }

        // Navigation Properties

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;

        [ForeignKey(nameof(CollegeId))]
        public College College { get; set; } = null!;
    }
}