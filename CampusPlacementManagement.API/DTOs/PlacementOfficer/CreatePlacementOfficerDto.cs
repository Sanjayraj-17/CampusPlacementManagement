using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.PlacementOfficer
{
    public class CreatePlacementOfficerDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "College ID is required.")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty;
    }
}
