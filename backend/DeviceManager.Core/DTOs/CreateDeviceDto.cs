using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DeviceManager.Core.DTOs;

public class CreateDeviceDto : IValidatableObject
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manufacturer is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Manufacturer must be between 1 and 100 characters")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required")]
    [RegularExpression("^(phone|tablet)$", ErrorMessage = "Type must be 'phone' or 'tablet'")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operating system is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Operating system must be between 1 and 50 characters")]
    public string OperatingSystem { get; set; } = string.Empty;

    [Required(ErrorMessage = "OS version is required")]
    [RegularExpression(@"^\d+\.\d+$", ErrorMessage = "OS version must be in the format 'major.minor' (e.g. 16.0)")]
    public string OsVersion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Processor is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Processor must be between 1 and 100 characters")]
    public string Processor { get; set; } = string.Empty;

    [Required(ErrorMessage = "RAM amount is required")]
    [Range(1, 1024, ErrorMessage = "RAM must be between 1 and 1024 GB")]
    public int RamAmount { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    public int? AssignedUserId { get; set; }

    // IValidatableObject lets you write cross-field rules that annotations can't express
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Name?.Trim().Length == 0)
            yield return new ValidationResult("Name cannot be whitespace", [nameof(Name)]);

        if (Manufacturer?.Trim().Length == 0)
            yield return new ValidationResult("Manufacturer cannot be whitespace", [nameof(Manufacturer)]);
    }
}