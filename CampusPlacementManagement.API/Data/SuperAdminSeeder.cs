
using CampusPlacementManagement.API.Enums;
using CampusPlacementManagement.API.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.API.Data
{
    public static class SuperAdminSeeder
    {
        public static async Task SeedSuperAdminAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Read bootstrap credentials from User Secrets
            var email = configuration["SuperAdminBootstrap:Email"];
            var password = configuration["SuperAdminBootstrap:Password"];
            var fullName = configuration["SuperAdminBootstrap:FullName"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException(
                    "SuperAdmin bootstrap settings are missing.");
            }

            // Check whether this email already exists
            var existingUser =
                await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                if (await userManager.IsInRoleAsync(
                    existingUser, "SuperAdmin"))
                {
                    return;
                }

                throw new InvalidOperationException(
                    "The configured SuperAdmin email already belongs " +
                    "to another account. No role was changed.");
            }

            // Create the initial SuperAdmin
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(
                admin, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "SuperAdmin creation failed: " +
                    string.Join(", ", createResult.Errors.Select(
                        e => e.Description)));
            }

            // Assign SuperAdmin role
            var roleResult = await userManager.AddToRoleAsync(
                admin, "SuperAdmin");

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(admin);

                throw new InvalidOperationException(
                    "SuperAdmin role assignment failed: " +
                    string.Join(", ", roleResult.Errors.Select(
                        e => e.Description)));
            }
        }
    }
}