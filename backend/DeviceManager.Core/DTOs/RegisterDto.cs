using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Core.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [StringLength(100)] public string Location { get; set; } = string.Empty;
}