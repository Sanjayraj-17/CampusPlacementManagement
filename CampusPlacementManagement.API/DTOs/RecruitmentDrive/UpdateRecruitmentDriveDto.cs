using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.API.Enums;

namespace CampusPlacementManagement.API.DTOs.RecruitmentDrive
{
    // DTO received when updating an existing recruitment drive
    public class UpdateRecruitmentDriveDto
    {
        [Required(ErrorMessage = "Drive date is required.")]
        public DateTime DriveDate { get; set; }

        [MaxLength(255, ErrorMessage = "Venue cannot exceed 255 characters.")]
        public string? Venue { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public RecruitmentDriveStatus Status { get; set; }
    }
}
