using CampusPlacementManagement.API.DTOs.College;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CollegesController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegesController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        // GET: api/colleges
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var colleges = await _collegeService.GetAllAsync();
            return Ok(colleges);
        }

        // GET: api/colleges/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var college = await _collegeService.GetByIdAsync(id);

            if (college == null)
                return NotFound(new { message = "College not found." });

            return Ok(college);
        }

        // POST: api/colleges
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CollegeRequestDto request)
        {
            var result = await _collegeService.CreateAsync(request);

            if (result == "College code already exists.")
                return Conflict(new { message = result });

            return Ok(new { message = result });
        }

        // PUT: api/colleges/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CollegeRequestDto request)
        {
            var result = await _collegeService.UpdateAsync(id, request);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result == "College code already exists.")
                return Conflict(new { message = result });

            return Ok(new { message = result });
        }

        // DELETE: api/colleges/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _collegeService.DeleteAsync(id);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result.Contains("cannot be deleted"))
                return Conflict(new { message = result });

            return Ok(new { message = result });
        }

        // PUT: api/colleges/1/approve
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _collegeService.ApproveAsync(id);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result.Contains("already"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }

        // PUT: api/colleges/1/reject
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _collegeService.RejectAsync(id);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result.Contains("already"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }

        // PUT: api/colleges/1/suspend
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            var result = await _collegeService.SuspendAsync(id);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result.Contains("already"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }

        // PUT: api/colleges/1/reactivate
        [HttpPut("{id:int}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var result = await _collegeService.ReactivateAsync(id);

            if (result == "College not found.")
                return NotFound(new { message = result });

            if (result.Contains("already"))
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }
    }
}
