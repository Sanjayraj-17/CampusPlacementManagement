
using CampusPlacementManagement.API.DTOs.Auth;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterUserAsync(
            RegisterRequestDto request,
            string role);

        Task<AuthResponseDto?> LoginAsync(
            LoginRequestDto request);
    }
}
