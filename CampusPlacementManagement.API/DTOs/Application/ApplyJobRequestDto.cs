using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Application
{
    // DTO received when a student applies for a job
    public class ApplyJobRequestDto
    {
        [Required(ErrorMessage = "Job ID is required.")]
        public int JobId { get; set; }

        [MaxLength(500, ErrorMessage = "Resume URL cannot exceed 500 characters.")]
        public string? ResumeUrl { get; set; }
    }
}
