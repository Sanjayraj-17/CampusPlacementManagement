using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class PlacementOfficerRepository : IPlacementOfficerRepository
    {
        private readonly ApplicationDbContext _context;

        public PlacementOfficerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all placement officers, optionally filtered by college
        public async Task<List<PlacementOfficer>> GetAllAsync(int? collegeId = null)
        {
            var query = _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .Include(p => p.ApprovedByUser)
                .AsNoTracking();

            if (collegeId.HasValue)
            {
                query = query.Where(p => p.CollegeId == collegeId.Value);
            }

            return await query
                .OrderBy(p => p.User.FullName)
                .ToListAsync();
        }

        // Get placement officer by ID
        public async Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId)
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .Include(p => p.ApprovedByUser)
                .FirstOrDefaultAsync(p => p.PlacementOfficerId == placementOfficerId);
        }

        // Get placement officer by ApplicationUser ID
        public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
        {
            return await _context.PlacementOfficers
                .Include(p => p.User)
                .Include(p => p.College)
                .Include(p => p.ApprovedByUser)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        // Add new placement officer
        public async Task AddAsync(PlacementOfficer placementOfficer)
        {
            await _context.PlacementOfficers.AddAsync(placementOfficer);
        }

        // Update placement officer
        public void Update(PlacementOfficer placementOfficer)
        {
            _context.PlacementOfficers.Update(placementOfficer);
        }

        // Delete placement officer
        public void Delete(PlacementOfficer placementOfficer)
        {
            _context.PlacementOfficers.Remove(placementOfficer);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
