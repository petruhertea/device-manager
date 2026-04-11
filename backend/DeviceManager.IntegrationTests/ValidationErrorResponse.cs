namespace DeviceManager.IntegrationTests;

public record ValidationErrorResponse(
    Dictionary<string, string[]> Errors,
    int Status,
    string Title
);