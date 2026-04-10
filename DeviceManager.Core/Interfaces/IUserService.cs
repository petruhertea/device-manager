using DeviceManager.Core.DTOs;

namespace DeviceManager.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    
    // TODO Phase 3: add RegisterAsync(RegisterDto) and LoginAsync(LoginDto) methods
}