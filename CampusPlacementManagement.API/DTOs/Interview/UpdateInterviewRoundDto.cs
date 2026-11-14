using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Interview
{
    // DTO received when updating an existing interview round
    public class UpdateInterviewRoundDto
    {
        [Required(ErrorMessage = "Round name is required.")]
        [MaxLength(100, ErrorMessage = "Round name cannot exceed 100 characters.")]
        public string RoundName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Round number is required.")]
        [Range(1, 20, ErrorMessage = "Round number must be between 1 and 20.")]
        public int RoundNumber { get; set; }

        public string? Description { get; set; }
    }
}
