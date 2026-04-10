namespace DeviceManager.Core.DTOs;

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public int RamAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? AssignedUserId { get; set; }
}