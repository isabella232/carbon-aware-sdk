namespace CarbonAware.WebApi.Models;

using CarbonAware.Model;
using System.Text.Json.Serialization;

[Serializable]
public record EmissionsForecastBatchDTO : EmissionsForecastBaseDTO
{
  [JsonPropertyName("requestedAt")]
  public DateTimeOffset RequestedAt { get; set; }
}