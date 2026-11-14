using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.RecruitmentDrive
{
    // DTO received when scheduling a new recruitment drive
    public class CreateRecruitmentDriveDto
    {
        [Required(ErrorMessage = "Job ID is required.")]
        public int JobId { get; set; }

        [Required(ErrorMessage = "College ID is required.")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Drive date is required.")]
        public DateTime DriveDate { get; set; }

        [MaxLength(255, ErrorMessage = "Venue cannot exceed 255 characters.")]
        public string? Venue { get; set; }

        public string? Description { get; set; }
    }
}
