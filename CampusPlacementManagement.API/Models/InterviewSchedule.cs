using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.Models
{
    public class InterviewSchedule
    {
        public int InterviewScheduleId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public int InterviewRoundId { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }

        [Required]
        public InterviewScheduleStatus Status { get; set; }
            = InterviewScheduleStatus.Scheduled;

        public string? Feedback { get; set; }

        // Navigation Properties

        [ForeignKey(nameof(ApplicationId))]
        public JobApplication JobApplication { get; set; } = null!;

        [ForeignKey(nameof(InterviewRoundId))]
        public InterviewRound InterviewRound { get; set; } = null!;
    }
}