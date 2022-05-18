using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Compute;
// using Azure.Monitor.Query;


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

    public Task<IEnumerable<PowerConsumptionData>> GetEnergyAsync(BaseComputeResource computeResource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        /// https://docs.microsoft.com/en-us/dotnet/api/overview/azure/monitor/management?view=azure-dotnet
        /// https://stackoverflow.com/questions/54327418/get-cpu-utilization-of-virtual-machines-in-azure-using-python-sdk
        /// https://docs.microsoft.com/en-us/azure/virtual-machines/linux/metrics-vm-usage-rest
        /// https://github.com/Azure/azure-sdk-for-python/issues/9885
        /// https://docs.microsoft.com/en-us/samples/azure-samples/monitor-dotnet-metrics-api/monitor-dotnet-metrics-api/

        // ArmClient armClient = new ArmClient(new DefaultAzureCredential());
        // Subscription subscription = await armClient.GetDefaultSubscriptionAsync();
        // string rgName = "myRgName";
        // ResourceGroup myRG = await subscription.GetResourceGroups().GetIfExistsAsync(rgName);

        // var credential = new DefaultAzureCredential();

        // var metricsQueryClient = new MetricsQueryClient(credential);
        var resource = computeResource as CloudComputeResource ?? throw new ArgumentException("computeResource must be of type CloudComputeResource");
        resource.Hardware = new ComputeHardware($"Azure.{resource.VmType}");
        
        if( resource.UtilizationData == null )
        {
            // TODO - get the utilization data from Azure
            throw new NotImplementedException("Fetching Usage metrics from Azure is not implemented yet");
        }

        return Task.FromResult(CalculationPowerConsumption(resource));
    }

    private IEnumerable<PowerConsumptionData> CalculationPowerConsumption(CloudComputeResource resource)
    {
        var cores = resource.Hardware.Cores;
        var powerPerCore = resource.Hardware.PowerPerCore;
        var resourceName = resource.Name;

        foreach (var data in resource.UtilizationData)
        {
            double kwhEnergy = (cores * powerPerCore * data.CpuUtilizationPercentage * data.Duration.TotalHours)/1000;
            yield return new PowerConsumptionData()
            {
                ComputeResourceName = resourceName,
                Energy = kwhEnergy,
                Timestamp = data.Timestamp,
                Duration = data.Duration
            };
        }
    }
}