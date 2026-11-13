
using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class CollegeRepository : ICollegeRepository
    {
        private readonly ApplicationDbContext _context;

        public CollegeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all colleges
        public async Task<List<College>> GetAllAsync()
        {
            return await _context.Colleges
                .AsNoTracking()
                .OrderBy(c => c.CollegeName)
                .ToListAsync();
        }

        // Get college by ID
        public async Task<College?> GetByIdAsync(int collegeId)
        {
            return await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        }

        // Get college by unique code
        public async Task<College?> GetByCodeAsync(string collegeCode)
        {
            return await _context.Colleges
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);
        }

        // Add college
        public async Task AddAsync(College college)
        {
            await _context.Colleges.AddAsync(college);
        }

        // Update college
        public void Update(College college)
        {
            _context.Colleges.Update(college);
        }

        // Delete college
        public void Delete(College college)
        {
            _context.Colleges.Remove(college);
        }

        // Save database changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
