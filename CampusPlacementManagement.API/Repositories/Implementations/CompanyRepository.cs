using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.API.Repositories.Implementations
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all companies, optionally filtered by status
        public async Task<List<Company>> GetAllAsync(CompanyStatus? status = null)
        {
            var query = _context.Companies.AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        // Get company by ID
        public async Task<Company?> GetByIdAsync(int companyId)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
        }

        // Get company by email (case-insensitive check)
        public async Task<Company?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            return await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyEmail.ToUpper() == normalizedEmail);
        }

        // Get company by name (case-insensitive check)
        public async Task<Company?> GetByNameAsync(string name)
        {
            var normalizedName = name.Trim().ToUpperInvariant();

            return await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyName.ToUpper() == normalizedName);
        }

        // Add new company
        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
        }

        // Update company
        public void Update(Company company)
        {
            _context.Companies.Update(company);
        }

        // Delete company
        public void Delete(Company company)
        {
            _context.Companies.Remove(company);
        }

        // Save changes to database
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
