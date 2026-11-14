using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.PlacementOfficer;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlacementOfficersController : ControllerBase
    {
        private readonly IPlacementOfficerService _placementOfficerService;

        public PlacementOfficersController(IPlacementOfficerService placementOfficerService)
        {
            _placementOfficerService = placementOfficerService;
        }

        // GET: api/placementofficers
        // SuperAdmin can view all placement officers, optionally filtered by collegeId
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] int? collegeId)
        {
            var officers = await _placementOfficerService.GetAllAsync(collegeId);
            return Ok(officers);
        }

        // GET: api/placementofficers/me
        // Logged-in placement officer can view their own profile
        [HttpGet("me")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var officer = await _placementOfficerService.GetByUserIdAsync(userId);
            if (officer == null)
            {
                return NotFound(new { message = "Placement officer profile not found." });
            }

            return Ok(officer);
        }

        // GET: api/placementofficers/1
        // SuperAdmin can view any profile; PlacementOfficer can view only their own
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> GetById(int id)
        {
            var officer = await _placementOfficerService.GetByIdAsync(id);
            if (officer == null)
            {
                return NotFound(new { message = "Placement officer not found." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            // Resource-based check: Officer can only view their own profile
            if (!isSuperAdmin && officer.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(officer);
        }

        // POST: api/placementofficers
        // SuperAdmin creates a new Placement Officer account and profile
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreatePlacementOfficerDto request)
        {
            var result = await _placementOfficerService.CreateAsync(request);

            if (result == "College not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Email is already registered.")
            {
                return Conflict(new { message = result });
            }

            if (result.Contains("unapproved college") || result.Contains("; "))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/placementofficers/1
        // SuperAdmin or the officer themselves can update profile details
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePlacementOfficerDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _placementOfficerService.UpdateAsync(
                id,
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "Placement officer not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to update this profile.")
            {
                return Forbid();
            }

            if (result == "Target college not found." || result == "Target college is not approved.")
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/placementofficers/1/approve
        // SuperAdmin approves a placement officer
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _placementOfficerService.ApproveAsync(id, currentUserId);

            if (result == "Placement officer not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/placementofficers/1/reject
        // SuperAdmin rejects a placement officer
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reject(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _placementOfficerService.RejectAsync(id, currentUserId);

            if (result == "Placement officer not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/placementofficers/1/suspend
        // SuperAdmin suspends a placement officer
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Suspend(int id)
        {
            var result = await _placementOfficerService.SuspendAsync(id);

            if (result == "Placement officer not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/placementofficers/1/reactivate
        // SuperAdmin reactivates a suspended placement officer
        [HttpPut("{id:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var result = await _placementOfficerService.ReactivateAsync(id);

            if (result == "Placement officer not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/placementofficers/1
        // SuperAdmin deletes a placement officer
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _placementOfficerService.DeleteAsync(id);

            if (result == "Placement officer not found.")
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
