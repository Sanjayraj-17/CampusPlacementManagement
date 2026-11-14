using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.Interview;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InterviewsController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        public InterviewsController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        // ==========================================
        // 1. INTERVIEW ROUND ENDPOINTS
        // ==========================================

        // POST: api/interviews/rounds
        // Company HR or SuperAdmin creates an interview round for a job
        [HttpPost("rounds")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> CreateRound([FromBody] CreateInterviewRoundDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.CreateRoundAsync(request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("already exists"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/interviews/rounds/5
        // Company HR or SuperAdmin updates an interview round
        [HttpPut("rounds/{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> UpdateRound(int id, [FromBody] UpdateInterviewRoundDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.UpdateRoundAsync(id, request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("already exists"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/interviews/rounds/5
        // Company HR or SuperAdmin deletes an interview round
        [HttpDelete("rounds/{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> DeleteRound(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.DeleteRoundAsync(id, currentUserId, isSuperAdmin);

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

        // GET: api/interviews/rounds/{id}
        // Retrieve single interview round details
        [HttpGet("rounds/{id:int}")]
        public async Task<IActionResult> GetRoundById(int id)
        {
            var round = await _interviewService.GetRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound(new { message = "Interview round not found." });
            }

            return Ok(round);
        }

        // GET: api/interviews/rounds/job/5
        // Retrieve all interview rounds configured for a job opening
        [HttpGet("rounds/job/{jobId:int}")]
        public async Task<IActionResult> GetRoundsByJobId(int jobId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var rounds = await _interviewService.GetRoundsByJobIdAsync(
                jobId,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent);

            return Ok(rounds);
        }

        // ==========================================
        // 2. INTERVIEW SCHEDULE ENDPOINTS
        // ==========================================

        // POST: api/interviews/schedules
        // Company HR or SuperAdmin schedules an interview for an applicant
        [HttpPost("schedules")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.ScheduleInterviewAsync(request, currentUserId, isSuperAdmin);

            if (result.Contains("not found"))
            {
                return NotFound(new { message = result });
            }

            if (result.Contains("not authorized"))
            {
                return Forbid();
            }

            if (result.Contains("Cannot schedule") ||
                result.Contains("does not belong") ||
                result.Contains("past"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/interviews/schedules/5
        // Company HR or SuperAdmin updates schedule date, venue, status, or records feedback
        [HttpPut("schedules/{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateInterviewScheduleDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.UpdateScheduleAsync(id, request, currentUserId, isSuperAdmin);

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

        // DELETE: api/interviews/schedules/5
        // Company HR or SuperAdmin deletes an interview schedule
        [HttpDelete("schedules/{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _interviewService.DeleteScheduleAsync(id, currentUserId, isSuperAdmin);

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

        // GET: api/interviews/schedules/my-schedules
        // Student views all their own scheduled interviews
        [HttpGet("schedules/my-schedules")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMySchedules()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var schedules = await _interviewService.GetMySchedulesAsync(currentUserId);
            return Ok(schedules);
        }

        // GET: api/interviews/schedules/job/5
        // Retrieve all interview schedules for a specific job
        [HttpGet("schedules/job/{jobId:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR,PlacementOfficer")]
        public async Task<IActionResult> GetSchedulesForJob(int jobId, [FromQuery] InterviewScheduleStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");

            var schedules = await _interviewService.GetSchedulesForJobAsync(
                jobId,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                status);

            return Ok(schedules);
        }

        // GET: api/interviews/schedules
        // Retrieve all interview schedules accessible to caller's role
        [HttpGet("schedules")]
        [Authorize(Roles = "SuperAdmin,CompanyHR,PlacementOfficer")]
        public async Task<IActionResult> GetAllSchedules([FromQuery] InterviewScheduleStatus? status)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");

            var schedules = await _interviewService.GetAllSchedulesAsync(
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                status);

            return Ok(schedules);
        }

        // GET: api/interviews/schedules/5
        // Retrieve single interview schedule details
        [HttpGet("schedules/{id:int}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isCompanyHR = User.IsInRole("CompanyHR");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var schedule = await _interviewService.GetScheduleByIdAsync(
                id,
                currentUserId,
                isSuperAdmin,
                isCompanyHR,
                isPlacementOfficer,
                isStudent);

            if (schedule == null)
            {
                return NotFound(new { message = "Interview schedule not found or access denied." });
            }

            return Ok(schedule);
        }
    }
}
