namespace DeviceManager.Core.Interfaces;

public interface IDescriptionGeneratorService
{
    /// <summary>
    /// Generates a concise, human-readable device description from its specs.
    /// Returns null if the LM Studio endpoint is unreachable or returns an error,
    /// allowing callers to fall back gracefully.
    /// </summary>
    Task<string?> GenerateAsync(
        string name,
        string manufacturer,
        string type,
        string operatingSystem,
        string processor,
        int ramAmount);
}