using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.API.DTOs.Student
{
    public class UpdateStudentDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree is required.")]
        [MaxLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required(ErrorMessage = "Graduation year is required.")]
        [Range(2000, 2100, ErrorMessage = "Enter a valid graduation year.")]
        public int GraduationYear { get; set; }

        [Required(ErrorMessage = "CGPA is required.")]
        [Range(0.00, 10.00, ErrorMessage = "CGPA must be between 0.00 and 10.00.")]
        public decimal CGPA { get; set; }

        [Required(ErrorMessage = "Active backlogs count is required.")]
        [Range(0, 50, ErrorMessage = "Active backlogs cannot be negative.")]
        public int ActiveBacklogs { get; set; }

        [Url(ErrorMessage = "Enter a valid resume URL.")]
        [MaxLength(500)]
        public string? ResumeUrl { get; set; }
    }
}
