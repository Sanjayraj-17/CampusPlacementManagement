using CampusPlacementManagement.API.DTOs.Student;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentResponseDto>> GetAllAsync(
            string currentUserId,
            bool isSuperAdmin,
            bool isPlacementOfficer,
            int? collegeId = null,
            string? department = null,
            int? graduationYear = null);

        Task<StudentResponseDto?> GetByIdAsync(
            int studentId,
            string currentUserId,
            bool isSuperAdmin,
            bool isPlacementOfficer,
            bool isStudent);

        Task<StudentResponseDto?> GetByUserIdAsync(string userId);

        Task<string> CreateAsync(
            CreateStudentDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> UpdateAsync(
            int studentId,
            UpdateStudentDto request,
            string currentUserId,
            bool isSuperAdmin);

        Task<string> UpdateResumeAsync(string userId, UpdateResumeDto request);

        Task<string> ApproveAsync(int studentId, string currentUserId, bool isSuperAdmin);

        Task<string> RejectAsync(int studentId, string currentUserId, bool isSuperAdmin);

        Task<string> SuspendAsync(int studentId, string currentUserId, bool isSuperAdmin);

        Task<string> ReactivateAsync(int studentId, string currentUserId, bool isSuperAdmin);

        Task<string> DeleteAsync(int studentId, string currentUserId, bool isSuperAdmin);
    }
}
