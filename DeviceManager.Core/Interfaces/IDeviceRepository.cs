using DeviceManager.Core.Models;

namespace DeviceManager.Core.Interfaces;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetAllAsync();
    Task<Device?> GetByIdAsync(int id);
    Task<Device> CreateAsync(Device device);
    Task<Device?> UpdateAsync(int id, Device device);
    Task<bool> DeleteAsync(int id);
}