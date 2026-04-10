using DeviceManager.Core.Models;

namespace DeviceManager.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
}