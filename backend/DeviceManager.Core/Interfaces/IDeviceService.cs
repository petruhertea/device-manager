using DeviceManager.Core.DTOs;

namespace DeviceManager.Core.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetAllAsync();
    Task<DeviceDto?> GetByIdAsync(int id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceDto?> UpdateAsync(int id, UpdateDeviceDto dto);
    Task<bool> DeleteAsync(int id);
}