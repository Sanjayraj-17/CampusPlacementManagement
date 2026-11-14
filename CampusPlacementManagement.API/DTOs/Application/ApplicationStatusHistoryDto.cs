using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Application
{
    // DTO representing an audit trail entry for an application's lifecycle
    public class ApplicationStatusHistoryDto
    {
        public int HistoryId { get; set; }

        public int ApplicationId { get; set; }

        public ApplicationStatus? PreviousStatus { get; set; }

        public ApplicationStatus NewStatus { get; set; }

        public string UpdatedByUserId { get; set; } = string.Empty;

        public string UpdatedByName { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
