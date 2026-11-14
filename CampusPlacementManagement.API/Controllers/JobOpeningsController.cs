using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.Job;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobOpeningsController : ControllerBase
    {
        private readonly IJobOpeningService _jobOpeningService;

        public JobOpeningsController(IJobOpeningService jobOpeningService)
        {
            _jobOpeningService = jobOpeningService;
        }

        // GET: api/jobopenings
        // Filtered automatically based on caller's role (SuperAdmin, CompanyHR, PlacementOfficer, Student)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] JobStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var jobs = await _jobOpeningService.GetAllAsync(
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent,
                status);

            return Ok(jobs);
        }

        // GET: api/jobopenings/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var job = await _jobOpeningService.GetByIdAsync(
                id,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent);

            if (job == null)
            {
                return NotFound(new { message = "Job opening not found or access denied." });
            }

            return Ok(job);
        }

        // POST: api/jobopenings
        // Company HR creates a job opening as draft
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Create([FromBody] CreateJobOpeningDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.CreateAsync(
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "Company not found." || result == "Company HR not found." || result == "Company HR profile not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not approved") || result.Contains("deadline") || result.Contains("target college"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/jobopenings/1
        // Company HR or SuperAdmin updates job opening
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateJobOpeningDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.UpdateAsync(
                id,
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "Job opening not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to update job openings from another company.")
            {
                return Forbid();
            }

            if (result.Contains("deadline") || result.Contains("target college"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/jobopenings/1/publish
        // Company HR or SuperAdmin publishes the job opening
        [HttpPut("{id:int}/publish")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Publish(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.PublishAsync(id, currentUserId, isSuperAdmin);

            if (result == "Job opening not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to publish job openings from another company.")
            {
                return Forbid();
            }

            if (result.Contains("already") || result.Contains("Only approved") || result.Contains("deadline"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/jobopenings/1/close
        // Company HR or SuperAdmin closes the job opening
        [HttpPut("{id:int}/close")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Close(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.CloseAsync(id, currentUserId, isSuperAdmin);

            if (result == "Job opening not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/jobopenings/1/cancel
        // Company HR or SuperAdmin cancels the job opening
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.CancelAsync(id, currentUserId, isSuperAdmin);

            if (result == "Job opening not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/jobopenings/1
        // Company HR or SuperAdmin deletes a job opening
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _jobOpeningService.DeleteAsync(id, currentUserId, isSuperAdmin);

            if (result == "Job opening not found.")
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("cannot be deleted"))
            {
                return Conflict(new { message = result });
            }

            return Ok(new { message = result });
        }
    }
}
