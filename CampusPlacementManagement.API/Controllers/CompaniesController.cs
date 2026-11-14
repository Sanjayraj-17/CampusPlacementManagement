using CampusPlacementManagement.API.DTOs.Company;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // GET: api/companies
        // SuperAdmin sees all or filters by status; other roles see approved companies
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CompanyStatus? status)
        {
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var companies = await _companyService.GetAllAsync(status, isSuperAdmin);
            return Ok(companies);
        }

        // GET: api/companies/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
            {
                return NotFound(new { message = "Company not found." });
            }

            return Ok(company);
        }

        // POST: api/companies
        // SuperAdmin registers a new company
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CompanyRequestDto request)
        {
            var result = await _companyService.CreateAsync(request);

            if (result == "Company email already exists." || result == "Company name already exists.")
            {
                return Conflict(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companies/1
        // SuperAdmin or CompanyHR updates company details
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyRequestDto request)
        {
            var result = await _companyService.UpdateAsync(id, request);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Company email already exists." || result == "Company name already exists.")
            {
                return Conflict(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companies/1/approve
        // SuperAdmin approves a company
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _companyService.ApproveAsync(id);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companies/1/reject
        // SuperAdmin rejects a company
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _companyService.RejectAsync(id);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companies/1/suspend
        // SuperAdmin suspends a company
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Suspend(int id)
        {
            var result = await _companyService.SuspendAsync(id);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companies/1/reactivate
        // SuperAdmin reactivates a suspended company
        [HttpPut("{id:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var result = await _companyService.ReactivateAsync(id);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/companies/1
        // SuperAdmin deletes a company
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _companyService.DeleteAsync(id);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("cannot be deleted"))
            {
                return Conflict(new { message = result });
            }

            return Ok(new { message = result });
        }
    }
}
