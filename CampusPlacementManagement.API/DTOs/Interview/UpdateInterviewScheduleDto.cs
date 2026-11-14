using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Interview
{
    // DTO received when updating an existing interview schedule or submitting feedback
    public class UpdateInterviewScheduleDto
    {
        [Required(ErrorMessage = "Scheduled date and time is required.")]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(500, ErrorMessage = "Meeting link cannot exceed 500 characters.")]
        public string? MeetingLink { get; set; }

        [MaxLength(255, ErrorMessage = "Venue cannot exceed 255 characters.")]
        public string? Venue { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public InterviewScheduleStatus Status { get; set; }

        public string? Feedback { get; set; }
    }
}
