using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.Application;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IJobApplicationService _applicationService;

        public JobApplicationsController(IJobApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // POST: api/jobapplications/apply
        // Student applies for a published job opening
        [HttpPost("apply")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var result = await _applicationService.ApplyAsync(request, currentUserId);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not approved") ||
                result.Contains("not currently") ||
                result.Contains("Please provide") ||
                result.Contains("passed") ||
                result.Contains("already applied") ||
                result.Contains("not open") ||
                result.Contains("does not meet") ||
                result.Contains("exceed") ||
                result.Contains("not eligible"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // GET: api/jobapplications/check-eligibility/5
        // Student verifies their eligibility before applying for a job
        [HttpGet("check-eligibility/{jobId:int}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckEligibility(int jobId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var result = await _applicationService.CheckEligibilityAsync(jobId, currentUserId);
            if (result == null)
            {
                return NotFound(new { message = "Job opening or student profile not found." });
            }

            return Ok(result);
        }

        // PUT: api/jobapplications/5/withdraw
        // Student withdraws their previously submitted job application
        [HttpPut("{id:int}/withdraw")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Withdraw(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var result = await _applicationService.WithdrawAsync(id, currentUserId);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("already") || result.Contains("Cannot withdraw"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // GET: api/jobapplications/my-applications
        // Student retrieves all job applications they have submitted
        [HttpGet("my-applications")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyApplications()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var applications = await _applicationService.GetMyApplicationsAsync(currentUserId);
            return Ok(applications);
        }

        // GET: api/jobapplications/job/5
        // Retrieve all applications for a specific job opening
        // Company HR: sees applications for their company's job
        // Placement Officer: sees applications for students of their college
        // SuperAdmin: sees all applications
        [HttpGet("job/{jobId:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR,PlacementOfficer")]
        public async Task<IActionResult> GetApplicationsForJob(int jobId, [FromQuery] ApplicationStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");

            var applications = await _applicationService.GetApplicationsForJobAsync(
                jobId,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                status);

            return Ok(applications);
        }

        // GET: api/jobapplications
        // Retrieve all applications accessible to the caller's role
        // Company HR: all applications across all company jobs
        // Placement Officer: all applications from students in their college
        // SuperAdmin: all applications across the platform
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyHR,PlacementOfficer")]
        public async Task<IActionResult> GetAll([FromQuery] ApplicationStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");

            var applications = await _applicationService.GetAllAsync(
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                status);

            return Ok(applications);
        }

        // GET: api/jobapplications/5
        // Retrieve single application details with audit history
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var application = await _applicationService.GetByIdAsync(
                id,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent);

            if (application == null)
            {
                return NotFound(new { message = "Job application not found or access denied." });
            }

            return Ok(application);
        }

        // PUT: api/jobapplications/5/status
        // Company HR or SuperAdmin updates application status
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _applicationService.UpdateStatusAsync(id, request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("Cannot update") || result.Contains("already in"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }
    }
}
