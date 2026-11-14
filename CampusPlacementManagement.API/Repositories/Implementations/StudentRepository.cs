using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all students with optional filters
        public async Task<List<Student>> GetAllAsync(
            int? collegeId = null,
            string? department = null,
            int? graduationYear = null)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                    .ThenInclude(p => p.User)
                .AsNoTracking();

            if (collegeId.HasValue)
            {
                query = query.Where(s => s.CollegeId == collegeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                var deptUpper = department.Trim().ToUpperInvariant();
                query = query.Where(s => s.Department.ToUpper() == deptUpper);
            }

            if (graduationYear.HasValue)
            {
                query = query.Where(s => s.GraduationYear == graduationYear.Value);
            }

            return await query
                .OrderBy(s => s.RegisterNumber)
                .ToListAsync();
        }

        // Get student by ID
        public async Task<Student?> GetByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        // Get student by ApplicationUser ID
        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Get student by College ID and Register Number (case-insensitive)
        public async Task<Student?> GetByRegisterNumberAsync(int collegeId, string registerNumber)
        {
            var regUpper = registerNumber.Trim().ToUpperInvariant();

            return await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.CollegeId == collegeId &&
                    s.RegisterNumber.ToUpper() == regUpper);
        }

        // Add new student
        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        // Update student
        public void Update(Student student)
        {
            _context.Students.Update(student);
        }

        // Delete student
        public void Delete(Student student)
        {
            _context.Students.Remove(student);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
