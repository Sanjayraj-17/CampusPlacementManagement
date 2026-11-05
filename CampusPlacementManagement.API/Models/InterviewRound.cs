using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementManagement.API.Models
{
    public class InterviewRound
    {
        public int InterviewRoundId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoundName { get; set; } = string.Empty;

        [Required]
        public int RoundNumber { get; set; }

        public string? Description { get; set; }

        // Navigation Property

        [ForeignKey(nameof(JobId))]
        public JobOpening JobOpening { get; set; } = null!;
    }
}
