using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.RecruitmentDrive;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RecruitmentDrivesController : ControllerBase
    {
        private readonly IRecruitmentDriveService _driveService;

        public RecruitmentDrivesController(IRecruitmentDriveService driveService)
        {
            _driveService = driveService;
        }

        // GET: api/recruitmentdrives
        // List recruitment drives filtered by caller's role and college/company boundary
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? jobId,
            [FromQuery] int? collegeId,
            [FromQuery] RecruitmentDriveStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var drives = await _driveService.GetAllAsync(
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent,
                jobId,
                collegeId,
                status);

            return Ok(drives);
        }

        // GET: api/recruitmentdrives/5
        // Retrieve single recruitment drive details
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var drive = await _driveService.GetByIdAsync(
                id,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent);

            if (drive == null)
            {
                return NotFound(new { message = "Recruitment drive not found or access denied." });
            }

            return Ok(drive);
        }

        // POST: api/recruitmentdrives
        // Company HR or SuperAdmin schedules a new recruitment drive
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Create([FromBody] CreateRecruitmentDriveDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _driveService.CreateAsync(request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("not approved") ||
                result.Contains("only be scheduled") ||
                result.Contains("not targeted") ||
                result.Contains("past"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/recruitmentdrives/5
        // Company HR or SuperAdmin updates recruitment drive details and status
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecruitmentDriveDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _driveService.UpdateAsync(id, request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            return Ok(new { message = result });
        }

        // DELETE: api/recruitmentdrives/5
        // Company HR or SuperAdmin cancels or removes a scheduled drive
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _driveService.DeleteAsync(id, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("Cannot delete"))
            {
                return Conflict(new { message = result });
            }

            return Ok(new { message = result });
        }
    }
}
