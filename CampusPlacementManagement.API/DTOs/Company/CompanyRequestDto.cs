using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Company
{
    public class CompanyRequestDto
    {
        [Required(ErrorMessage = "Company name is required.")]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(255)]
        public string CompanyEmail { get; set; } = string.Empty;

        [Url(ErrorMessage = "Enter a valid website URL.")]
        [MaxLength(255)]
        public string? Website { get; set; }

        [Required(ErrorMessage = "Industry is required.")]
        [MaxLength(100)]
        public string Industry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
