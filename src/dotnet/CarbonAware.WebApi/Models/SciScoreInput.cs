using System.Text.Json.Serialization;
using CarbonAware.Interfaces;

namespace CarbonAware.WebApi.Models;

[Serializable]
public record SciScoreInput
{
    [JsonPropertyName("location")]
    public LocationInput? Location { get; set; }

    [JsonPropertyName("resources")]
    public IEnumerable<ResourceInput>? Resources { get; set; }

    [JsonPropertyName("timeInterval")]
    public string TimeInterval { get; set; } = string.Empty;
}

[Serializable]
public record LocationInput
{
    [JsonPropertyName("locationType")]
    public string LocationType { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("cloudProvider")]
    public string? CloudProvider { get; set; }

    [JsonPropertyName("regionName")]
    public string? RegionName { get; set; }
}

[Serializable]
public record ResourceInput : IComputeResource
{
    private Dictionary<string, string>? _properties;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("processor")]
    public string? Processor {
        get => Properties.GetValueOrDefault("processor");
        set => Properties.Add("processor", value ?? string.Empty);
    }

    public Dictionary<string, string> Properties {
        get => _properties ??= new Dictionary<string, string>();
        set => _properties = value;
    }
}