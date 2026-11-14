using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class CompanyHRRepository : ICompanyHRRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyHRRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all company HRs, optionally filtered by companyId
        public async Task<List<CompanyHR>> GetAllAsync(int? companyId = null)
        {
            var query = _context.CompanyHRs
                .Include(h => h.User)
                .Include(h => h.Company)
                .Include(h => h.ApprovedByUser)
                .AsNoTracking();

            if (companyId.HasValue)
            {
                query = query.Where(h => h.CompanyId == companyId.Value);
            }

            return await query
                .OrderBy(h => h.User.FullName)
                .ToListAsync();
        }

        // Get company HR by ID
        public async Task<CompanyHR?> GetByIdAsync(int companyHRId)
        {
            return await _context.CompanyHRs
                .Include(h => h.User)
                .Include(h => h.Company)
                .Include(h => h.ApprovedByUser)
                .FirstOrDefaultAsync(h => h.CompanyHRId == companyHRId);
        }

        // Get company HR by ApplicationUser ID
        public async Task<CompanyHR?> GetByUserIdAsync(string userId)
        {
            return await _context.CompanyHRs
                .Include(h => h.User)
                .Include(h => h.Company)
                .Include(h => h.ApprovedByUser)
                .FirstOrDefaultAsync(h => h.UserId == userId);
        }

        // Add new company HR
        public async Task AddAsync(CompanyHR companyHR)
        {
            await _context.CompanyHRs.AddAsync(companyHR);
        }

        // Update company HR
        public void Update(CompanyHR companyHR)
        {
            _context.CompanyHRs.Update(companyHR);
        }

        // Delete company HR
        public void Delete(CompanyHR companyHR)
        {
            _context.CompanyHRs.Remove(companyHR);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
