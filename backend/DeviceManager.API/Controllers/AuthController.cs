using System.IdentityModel.Tokens.Jwt;
using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
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
                Secure   = false,    // set to true in production (HTTPS only)
                SameSite = SameSiteMode.Strict,
                Expires  = DateTimeOffset.UtcNow.AddMinutes(60)
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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult<AuthUserDto> Me()
    {
        var id       = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var email    = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role     = User.FindFirst("role")?.Value;

        return Ok(new AuthUserDto
        {
            Id       = int.Parse(id ?? "0"),
            Email    = email ?? "",
            FullName = fullName ?? "",
            Role     = role ?? ""
        });
    }
}