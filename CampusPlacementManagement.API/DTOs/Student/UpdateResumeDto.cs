using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Student
{
    public class UpdateResumeDto
    {
        [Required(ErrorMessage = "Resume URL is required.")]
        [Url(ErrorMessage = "Enter a valid resume URL.")]
        [MaxLength(500)]
        public string ResumeUrl { get; set; } = string.Empty;
    }
}
