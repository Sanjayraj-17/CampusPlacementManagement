using CampusPlacementManagement.API.Data;
using CampusPlacementManagement.API.Models;
using CampusPlacementManagement.API.Repositories.Implementations;
using CampusPlacementManagement.API.Repositories.Interfaces;
using CampusPlacementManagement.API.Services.Implementations;
using CampusPlacementManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ASP.NET Core Identity Configuration
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password Settings
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // User Settings
        options.User.RequireUniqueEmail = true;

        // Lockout Settings
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Configuration
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            ),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// JWT Service Registration
builder.Services.AddScoped<IJwtService, JwtService>();

// Authentication Service Registration
builder.Services.AddScoped<IAuthService, AuthService>();

// College Repository Registration
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();

// College Service Registration
builder.Services.AddScoped<ICollegeService, CollegeService>();

// Placement Officer Repository Registration
builder.Services.AddScoped<IPlacementOfficerRepository, PlacementOfficerRepository>();

// Placement Officer Service Registration
builder.Services.AddScoped<IPlacementOfficerService, PlacementOfficerService>();

// Company Repository Registration
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

// Company Service Registration
builder.Services.AddScoped<ICompanyService, CompanyService>();

// Company HR Repository Registration
builder.Services.AddScoped<ICompanyHRRepository, CompanyHRRepository>();

// Company HR Service Registration
builder.Services.AddScoped<ICompanyHRService, CompanyHRService>();



builder.Services.AddControllers();

builder.Services.AddOpenApi();
var app = builder.Build();

// Seed Identity Roles and Initial SuperAdmin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // First create the four Identity roles
    await RoleSeeder.SeedRolesAsync(services);

    // Then create the initial SuperAdmin
    await SuperAdminSeeder.SeedSuperAdminAsync(
        services,
        app.Configuration);
}



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();