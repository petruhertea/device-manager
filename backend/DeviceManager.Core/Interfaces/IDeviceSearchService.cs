using DeviceManager.Core.DTOs;

namespace DeviceManager.Core.Interfaces;

public interface IDeviceSearchService
{
    /// <summary>
    /// Returns devices matching <paramref name="query"/> ordered by relevance score,
    /// highest first. Returns an empty list (never null) when nothing matches.
    /// </summary>
    Task<IEnumerable<DeviceSearchResultDto>> SearchAsync(string query);
}