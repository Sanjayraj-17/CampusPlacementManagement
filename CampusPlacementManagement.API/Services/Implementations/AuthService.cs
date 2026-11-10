
using CampusPlacementManagement.API.DTOs.Auth;
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        // REGISTER USER
        public async Task<string> RegisterUserAsync(
            RegisterRequestDto request,
            string role)
        {
            // Only allow roles assigned through controlled workflows.
            string[] allowedRoles =
            {
                "PlacementOfficer",
                "CompanyHR",
                "Student"
            };

            if (!allowedRoles.Contains(role))
            {
                return "Invalid role assignment.";
            }

            // Check whether email already exists.
            var existingUser = await _userManager.FindByEmailAsync(
                request.Email);

            if (existingUser != null)
            {
                return "Email is already registered.";
            }

            // Create new user.
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Identity hashes the password automatically.
            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                return string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));
            }

            // Assign the requested role.
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                role);

            if (!roleResult.Succeeded)
            {
                // Remove the user if role assignment fails.
                await _userManager.DeleteAsync(user);

                return string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));
            }

            return "User registered successfully. Account is pending approval.";
        }

        // LOGIN USER
        public async Task<AuthResponseDto?> LoginAsync(
            LoginRequestDto request)
        {
            // Find user using email.
            var user = await _userManager.FindByEmailAsync(
                request.Email);

            if (user == null)
            {
                return null;
            }

            // Validate password.
            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                request.Password);

            if (!passwordValid)
            {
                return null;
            }

            // Only approved accounts can log in.
            if (user.AccountStatus != AccountStatus.Approved)
            {
                return null;
            }

            // Retrieve assigned roles.
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT.
            var token = _jwtService.GenerateToken(
                user,
                roles);

            var expiryMinutes =
                _configuration.GetValue<int>("Jwt:ExpiryMinutes");

            // Return authentication response.
            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }
    }
}
