namespace CarbonAware.DataSources.Azure.Models;

/// <summary>
/// Represents the compute resource to query for usage metrics.
/// </summary>
public class PowerConsumptionData
{
    public string ComputeResourceName { get; set; }

    public double Energy { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public TimeSpan Duration { get; set; }
}