using DeviceManager.Core.DTOs;

namespace DeviceManager.Core.Interfaces;

public interface IAuthService
{
    Task<AuthUserDto> RegisterAsync(RegisterDto dto);
    Task<(AuthUserDto user, string token)> LoginAsync(LoginDto dto);
}