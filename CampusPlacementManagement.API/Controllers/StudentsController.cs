using System.Security.Claims;
using CampusPlacementManagement.API.DTOs.Student;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/students
        // SuperAdmin sees all or filters; PlacementOfficer sees only their own college's students
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,CompanyHR")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? collegeId,
            [FromQuery] string? department,
            [FromQuery] int? graduationYear)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");

            var students = await _studentService.GetAllAsync(
                currentUserId,
                isSuperAdmin,
                isPlacementOfficer,
                collegeId,
                department,
                graduationYear);

            return Ok(students);
        }

        // GET: api/students/me
        // Logged-in student views their own profile
        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var student = await _studentService.GetByUserIdAsync(userId);
            if (student == null)
            {
                return NotFound(new { message = "Student profile not found." });
            }

            return Ok(student);
        }

        // GET: api/students/1
        // SuperAdmin can view any; Officer views only their own college; Student views only themselves
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student,CompanyHR")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var isPlacementOfficer = User.IsInRole("PlacementOfficer");
            var isStudent = User.IsInRole("Student");

            var student = await _studentService.GetByIdAsync(
                id,
                currentUserId,
                isSuperAdmin,
                isPlacementOfficer,
                isStudent);

            if (student == null)
            {
                return NotFound(new { message = "Student not found or access denied." });
            }

            return Ok(student);
        }

        // POST: api/students
        // Placement Officer creates a student in their college; SuperAdmin can specify college
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.CreateAsync(
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "College not found." || result == "Placement officer not found." || result == "Placement officer profile not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Email is already registered." || result == "Register number already exists for this college.")
            {
                return Conflict(new { message = result });
            }

            if (result.Contains("unapproved") || result.Contains("does not belong") || result.Contains("; "))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/1
        // Placement Officer or SuperAdmin updates student details
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.UpdateAsync(
                id,
                request,
                currentUserId,
                isSuperAdmin);

            if (result == "Student not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to update students from another college.")
            {
                return Forbid();
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/me/resume
        // Student updates their own resume URL
        [HttpPut("me/resume")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateResume([FromBody] UpdateResumeDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _studentService.UpdateResumeAsync(userId, request);

            if (result == "Student profile not found.")
            {
                return NotFound(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/1/approve
        // Placement Officer or SuperAdmin approves a student
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.ApproveAsync(id, currentUserId, isSuperAdmin);

            if (result == "Student not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to approve students from another college.")
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/1/reject
        // Placement Officer or SuperAdmin rejects a student
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Reject(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.RejectAsync(id, currentUserId, isSuperAdmin);

            if (result == "Student not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to reject students from another college.")
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/1/suspend
        // Placement Officer or SuperAdmin suspends a student
        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Suspend(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.SuspendAsync(id, currentUserId, isSuperAdmin);

            if (result == "Student not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to suspend students from another college.")
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // PUT: api/students/1/reactivate
        // Placement Officer or SuperAdmin reactivates a student
        [HttpPut("{id:int}/reactivate")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.ReactivateAsync(id, currentUserId, isSuperAdmin);

            if (result == "Student not found.")
            {
                return NotFound(new { message = result });
            }

            if (result == "You are not authorized to reactivate students from another college.")
            {
                return Forbid();
            }

            if (result.Contains("already"))
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        // DELETE: api/students/1
        // Placement Officer or SuperAdmin deletes a student
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var isSuperAdmin = User.IsInRole("SuperAdmin");

            var result = await _studentService.DeleteAsync(id, currentUserId, isSuperAdmin);

            if (result == "Student not found.")
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
