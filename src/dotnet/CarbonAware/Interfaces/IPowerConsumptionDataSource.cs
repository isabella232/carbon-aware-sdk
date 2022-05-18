namespace CarbonAware.Interfaces;

/// <summary>
/// Represents a data source for power consumption metrics.
/// </summary>
public interface IPowerConsumptionDataSource
{
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string Version { get; }

    /// <summary>
    /// Gets the energy used by compute resources within a given and start and end time
    /// </summary>
    /// <param name="computeResource">The computeResources that should be used for getting usage metrics.</param>
    /// <param name="periodStartTime">The start time of the period.</param>
    /// <param name="periodEndTime">The end time of the period.</param>
    /// <returns>A list of calculated energy consumption values for each resource.</returns>
    public Task<IEnumerable<PowerConsumptionData>> GetEnergyAsync(BaseComputeResource computeResource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);
}
