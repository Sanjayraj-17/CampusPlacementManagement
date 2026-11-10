
using CampusPlacementManagement.API.Models;

namespace CampusPlacementManagement.API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(
            ApplicationUser user,
            IList<string> roles
        );
    }
}
