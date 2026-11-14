using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.CompanyHR
{
    public class UpdateCompanyHRDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Designation { get; set; }

        public int? CompanyId { get; set; }
    }
}
