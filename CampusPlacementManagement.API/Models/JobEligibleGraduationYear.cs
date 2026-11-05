using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementManagement.API.Models
{
    public class JobEligibleGraduationYear
    {
        public int JobEligibleGraduationYearId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int GraduationYear { get; set; }

        // Navigation Property

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;
    }
}