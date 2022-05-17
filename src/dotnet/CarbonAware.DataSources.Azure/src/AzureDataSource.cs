using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarbonAware.DataSources.Azure;

/// <summary>
/// Represents an Azure data source.
/// </summary>
public class AzureDataSource : IPowerConsumptionDataSource
{
    public string Name => "AzureDataSource";

    public string Description => "Access metadata for Azure resources";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    private readonly ILogger<AzureDataSource> Logger;

    private ActivitySource ActivitySource { get; }



    /// <summary>
    /// Creates a new instance of the <see cref="AzureDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    public AzureDataSource(ILogger<AzureDataSource> logger, ActivitySource activitySource )
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.ActivitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    public Task<IEnumerable<double>> GetEnergyAsync(IEnumerable<ComputeResource> computeResources, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        // ArmClient armClient = new ArmClient(new DefaultAzureCredential());
        // Subscription subscription = await armClient.GetDefaultSubscriptionAsync();
        // string rgName = "myRgName";
        // ResourceGroup myRG = await subscription.GetResourceGroups().GetIfExistsAsync(rgName);
        List<double> results = new List<double>();
        results.Add(99.99);
        return Task.FromResult(results as IEnumerable<double>);
    }
}