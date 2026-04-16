using System.Security.Claims;
using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthUserDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await authService.RegisterAsync(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthUserDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var (user, token) = await authService.LoginAsync(dto);

            // Set token as HttpOnly cookie — JS cannot read this
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // set to true in production (HTTPS only)
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<AuthUserDto> Me()
    {
        var id = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var fullName = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst("role")?.Value;

        return Ok(new AuthUserDto
        {
            Id = int.TryParse(id, out var parsedId) ? parsedId : 0,
            Email = email ?? string.Empty,
            FullName = fullName ?? string.Empty,
            Role = role ?? string.Empty,
            Location = string.Empty
        });
    }
}