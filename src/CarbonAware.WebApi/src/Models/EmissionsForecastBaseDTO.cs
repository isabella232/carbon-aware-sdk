namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBase
{
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTimeOffset EndTime { get; set; }

    [JsonPropertyName("windowSize")]
    public int WindowSize { get; set; }
}