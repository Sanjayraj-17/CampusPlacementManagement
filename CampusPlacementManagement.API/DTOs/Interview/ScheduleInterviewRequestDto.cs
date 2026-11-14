using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Interview
{
    // DTO received when scheduling an interview for a candidate
    public class ScheduleInterviewRequestDto
    {
        [Required(ErrorMessage = "Application ID is required.")]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "Interview Round ID is required.")]
        public int InterviewRoundId { get; set; }

        [Required(ErrorMessage = "Scheduled date and time is required.")]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(500, ErrorMessage = "Meeting link cannot exceed 500 characters.")]
        public string? MeetingLink { get; set; }

        [MaxLength(255, ErrorMessage = "Venue cannot exceed 255 characters.")]
        public string? Venue { get; set; }
    }
}
