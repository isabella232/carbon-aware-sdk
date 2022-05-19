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
    /// <returns>A double of calculated energy consumption values for that resource.</returns>
    public Task<double> GetEnergyAsync(IComputeResource computeResource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);
}
