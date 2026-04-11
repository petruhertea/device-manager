using Microsoft.AspNetCore.Identity;

namespace DeviceManager.Core.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public string Location { get; set; } = string.Empty;
    
    public ICollection<Device> AssignedDevices { get; set; } = new List<Device>();
}