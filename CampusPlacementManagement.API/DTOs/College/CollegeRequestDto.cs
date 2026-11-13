
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.College
{
    public class CollegeRequestDto
    {
        [Required(ErrorMessage = "College name is required.")]
        [MaxLength(200)]
        public string CollegeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "College code is required.")]
        [MaxLength(50)]
        public string CollegeCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? UniversityName { get; set; }

        [Required(ErrorMessage = "Official email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(255)]
        public string OfficialEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Url(ErrorMessage = "Enter a valid website URL.")]
        [MaxLength(255)]
        public string? Website { get; set; }
    }
}
