using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.Application
{
    // DTO received when Company HR or SuperAdmin updates the status of an application
    public class UpdateApplicationStatusDto
    {
        [Required(ErrorMessage = "New status is required.")]
        public ApplicationStatus NewStatus { get; set; }

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
