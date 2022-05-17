using System.Text.Json.Serialization;

namespace CarbonAware.Model;

/// <summary>
/// Represents the compute resource to query for usage metrics.
/// </summary>
[Serializable]
public class ComputeResource
{
    // "metricSource": "Azure"
    // "subscriptionId": "123",
    // "resourceGroup": "rg-mygroup",
    // "type": "Microsoft.Compute/virtualMachines",
    // "name": "vm-MyVirtualMachine"

    [JsonPropertyName("metricSource")]
    public string MetricSource { get; set; } = "Azure";

    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;

    [JsonPropertyName("resourceGroup")]
    public string ResourceGroup { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Microsoft.Compute/virtualMachines";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}