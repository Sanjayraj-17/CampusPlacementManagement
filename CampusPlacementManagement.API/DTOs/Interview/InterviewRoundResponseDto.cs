namespace CampusPlacementManagement.API.DTOs.Interview
{
    // DTO returned when retrieving interview round information
    public class InterviewRoundResponseDto
    {
        public int InterviewRoundId { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string RoundName { get; set; } = string.Empty;

        public int RoundNumber { get; set; }

        public string? Description { get; set; }
    }
}
