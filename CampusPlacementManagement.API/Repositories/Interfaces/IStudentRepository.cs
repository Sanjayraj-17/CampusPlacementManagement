using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllAsync(
            int? collegeId = null,
            string? department = null,
            int? graduationYear = null);

        Task<Student?> GetByIdAsync(int studentId);

        Task<Student?> GetByUserIdAsync(string userId);

        Task<Student?> GetByRegisterNumberAsync(int collegeId, string registerNumber);

        Task AddAsync(Student student);

        void Update(Student student);

        void Delete(Student student);

        Task SaveChangesAsync();
    }
}
