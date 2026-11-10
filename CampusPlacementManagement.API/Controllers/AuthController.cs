
using CampusPlacementManagement.API.DTOs.Auth;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // 1. USER LOGIN
        // POST: api/auth/login
        // ==========================================

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid credentials or account is not approved."
                });
            }

            return Ok(result);
        }

        // ==========================================
        // 2. REGISTER PLACEMENT OFFICER / COMPANY HR
        // POST: api/auth/register-account?role=PlacementOfficer
        // POST: api/auth/register-account?role=CompanyHR
        // ==========================================

        [HttpPost("register-account")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RegisterAccount(
            [FromBody] RegisterRequestDto request,
            [FromQuery] string role)
        {
            if (role != "PlacementOfficer" && role != "CompanyHR")
            {
                return BadRequest(new
                {
                    message = "Only PlacementOfficer and CompanyHR accounts can be created through this endpoint."
                });
            }

            var result = await _authService.RegisterUserAsync(
                request,
                role);

            if (result != "User registered successfully. Account is pending approval.")
            {
                return BadRequest(new
                {
                    message = result
                });
            }

            return Ok(new
            {
                message = result
            });
        }
    }
}
