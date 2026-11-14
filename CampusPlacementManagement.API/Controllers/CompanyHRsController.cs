using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.CompanyHR;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyHRsController : ControllerBase
    {
        private readonly ICompanyHRService _companyHRService;

        public CompanyHRsController(ICompanyHRService companyHRService)
        {
            _companyHRService = companyHRService;
        }

        // GET: api/companyhrs
        // SuperAdmin can view all company HRs, optionally filtered by companyId
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] int? companyId)
        {
            var hrs = await _companyHRService.GetAllAsync(companyId);
            return Ok(hrs);
        }

        // GET: api/companyhrs/me
        // Logged-in Company HR can view their own profile
        [HttpGet("me")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var hr = await _companyHRService.GetByUserIdAsync(userId);
            if (hr == null)
            {
                return NotFound(new { message = "Company HR profile not found." });
            }

            return Ok(hr);
        }

        // GET: api/companyhrs/1
        // SuperAdmin can view any profile; CompanyHR can view only their own
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> GetById(int id)
        {
            var hr = await _companyHRService.GetByIdAsync(id);
            if (hr == null)
            {
                return NotFound(new { message = "Company HR not found." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            // Resource-based check: HR can only view their own profile
            if (!isSuperAdmin && hr.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(hr);
        }

        // POST: api/companyhrs
        // SuperAdmin creates a new Company HR account and profile
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateCompanyHRDto request)
        {
            var result = await _companyHRService.CreateAsync(request);

            if (result == "Company not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Email is already registered.")
            {
                return Conflict(new { message = result });
            }

            if (result.Contains("unapproved company") || result.Contains("; "))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companyhrs/1
        // SuperAdmin or the HR themselves can update profile details
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCompanyHRDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _companyHRService.UpdateAsync(
                id,
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "Company HR not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to update this profile.")
            {
                return Forbid();
            }

            if (result == "Target company not found." || result == "Target company is not approved.")
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companyhrs/1/approve
        // SuperAdmin approves a Company HR account
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _companyHRService.ApproveAsync(id, currentUserId);

            if (result == "Company HR not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companyhrs/1/reject
        // SuperAdmin rejects a Company HR account
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _companyHRService.RejectAsync(id, currentUserId);

            if (result == "Company HR not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companyhrs/1/suspend
        // SuperAdmin suspends a Company HR account
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Suspend(int id)
        {
            var result = await _companyHRService.SuspendAsync(id);

            if (result == "Company HR not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/companyhrs/1/reactivate
        // SuperAdmin reactivates a suspended Company HR account
        [HttpPut("{id:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var result = await _companyHRService.ReactivateAsync(id);

            if (result == "Company HR not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/companyhrs/1
        // SuperAdmin deletes a Company HR account
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _companyHRService.DeleteAsync(id);

            if (result == "Company HR not found.")
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
