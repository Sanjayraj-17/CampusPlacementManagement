using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.PlacementOfficer
{
    public class UpdatePlacementOfficerDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required.")]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty;

        public int? CollegeId { get; set; }
    }
}
