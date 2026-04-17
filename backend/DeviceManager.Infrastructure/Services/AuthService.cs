using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DeviceManager.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IAuthService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<AuthUserDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email, // Identity requires UserName
            Role = "Employee", // default role on registration
            Location = dto.Location
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        return ToDto(user);
    }

    public async Task<(AuthUserDto user, string token)> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email)
                   ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwt(user);
        return (ToDto(user), token);
    }

    private string GenerateJwt(ApplicationUser user)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                     ?? _configuration["JwtSettings:Key"]
                     ?? throw new InvalidOperationException("JWT_KEY is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email!),
            new Claim("name", user.FullName),
            new Claim("role", user.Role),
            new Claim("location", user.Location ?? string.Empty),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthUserDto ToDto(ApplicationUser u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email!,
        Role = u.Role,
        Location = u.Location
    };
}