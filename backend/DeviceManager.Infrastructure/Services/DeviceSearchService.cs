using System.Text.RegularExpressions;
using DeviceManager.Core.DTOs;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;

namespace DeviceManager.Infrastructure.Services;

/// <summary>
/// Relevance-ranked free-text device search.
///
/// Scoring weights (per token, per field):
///   Name         → 8 pts   (most specific — "Galaxy S24" should beat "Galaxy Tab S9")
///   Manufacturer → 5 pts   ("Samsung" matches many; still important for brand searches)
///   Processor    → 3 pts   ("Snapdragon" narrows down meaningfully)
///   RAM          → 2 pts   ("8" or "8GB" is a weaker signal — many devices share it)
///
/// A token must appear as a whole word (word-boundary match) to score. This prevents
/// "8" from matching "8 Gen 3" mid-token and inflating Snapdragon devices when
/// the user searches for RAM.
/// </summary>
public partial class DeviceSearchService(IDeviceRepository repository) : IDeviceSearchService
{
    // Scoring weights — easy to tune without touching logic
    private const int NameWeight         = 8;
    private const int ManufacturerWeight = 5;
    private const int ProcessorWeight    = 3;
    private const int RamWeight          = 2;

    public async Task<IEnumerable<DeviceSearchResultDto>> SearchAsync(string query)
    {
        // ── 1. Normalise and tokenise the query ──────────────────────────────
        var tokens = Tokenise(query);
        if (tokens.Length == 0)
            return [];

        // ── 2. Load all devices (with their assigned user) ───────────────────
        var devices = await repository.GetAllAsync();

        // ── 3. Score each device ─────────────────────────────────────────────
        var results = devices
            .Select(d => Score(d, tokens))
            .Where(r => r.Score > 0)
            .OrderByDescending(r => r.Score)
            .ToList();

        return results;
    }

    // ── Scoring ───────────────────────────────────────────────────────────────

    private static DeviceSearchResultDto Score(Device device, string[] tokens)
    {
        int score = 0;

        // RAM is exposed as an int; convert to string so token matching works uniformly
        var ramString = device.RamAmount.ToString();

        foreach (var token in tokens)
        {
            score += CountMatches(device.Name,          token) * NameWeight;
            score += CountMatches(device.Manufacturer,  token) * ManufacturerWeight;
            score += CountMatches(device.Processor,     token) * ProcessorWeight;
            score += CountMatches(ramString,            token) * RamWeight;
        }

        return new DeviceSearchResultDto
        {
            Id               = device.Id,
            Name             = device.Name,
            Manufacturer     = device.Manufacturer,
            Type             = device.Type,
            OperatingSystem  = device.OperatingSystem,
            OsVersion        = device.OsVersion,
            Processor        = device.Processor,
            RamAmount        = device.RamAmount,
            Description      = device.Description,
            AssignedUserName = device.AssignedUser?.FullName,
            Score            = score
        };
    }

    /// <summary>
    /// Counts how many times <paramref name="token"/> appears as a whole word
    /// inside <paramref name="field"/> (case-insensitive).
    /// </summary>
    private static int CountMatches(string field, string token)
    {
        if (string.IsNullOrEmpty(field)) return 0;

        // \b = word boundary so "8" doesn't match the "8" inside "18" or "8 Gen 3" mid-word
        var pattern = $@"\b{Regex.Escape(token)}\b";
        return Regex.Matches(field, pattern, RegexOptions.IgnoreCase).Count;
    }

    // ── Normalisation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Lowercases, strips punctuation, collapses whitespace, and splits into tokens.
    /// "Snapdragon 8, Gen 3" → ["snapdragon", "8", "gen", "3"]
    /// </summary>
    private static string[] Tokenise(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        // Replace punctuation and extra whitespace with a single space, then split
        var normalised = PunctuationRegex().Replace(query.Trim(), " ");
        return normalised
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()           // deduplicate — "samsung samsung" shouldn't double-score
            .ToArray();
    }

    [GeneratedRegex(@"[^\w\s]|\s+")]
    private static partial Regex PunctuationRegex();
}