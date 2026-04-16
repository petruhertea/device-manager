namespace DeviceManager.Core.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "phone" or "tablet"
    public string OperatingSystem { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public int RamAmount { get; set; } // in GB
    public string Description { get; set; } = string.Empty;

    // Foreign key — nullable because a device may be unassigned
    public int? AssignedUserId { get; set; }
    public ApplicationUser? AssignedUser { get; set; }
}