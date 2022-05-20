using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.DataSources.Azure.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;


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

    private Dictionary<string, VmSizeDetails> VmSizeRepository { get; }
    private Dictionary<string, Processor> ProcessorRepository { get; }



    /// <summary>
    /// Creates a new instance of the <see cref="AzureDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="activitySource">The activity source for telemetry.</param>
    public AzureDataSource(ILogger<AzureDataSource> logger, ActivitySource activitySource )
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.ActivitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        this.VmSizeRepository = new Dictionary<string, VmSizeDetails>();
        this.ProcessorRepository = new Dictionary<string, Processor>();
        initializeVmSizeRepository();
    }

    // TODO(bderusha) Remove this after resolver/emissions is abstracted
    public AzureDataSource()
    {
        this.Logger = new Logger<AzureDataSource>(new LoggerFactory());
        this.ActivitySource = new ActivitySource("AzureDataSource");
        this.VmSizeRepository = new Dictionary<string, VmSizeDetails>();
        this.ProcessorRepository = new Dictionary<string, Processor>();
        initializeVmSizeRepository();
    }

    public async Task<IEnumerable<CloudComputeResource>> ResolveResourcesAsync(IEnumerable<IComputeResource> resources)
    {
        var computeResources = new List<CloudComputeResource>();
        foreach (var resource in resources)
        {
            var computeResource = await this.ResolveResourceAsync(resource);
            if (computeResource != null)
            {
                computeResources.Add(computeResource);
            }
        }

        return computeResources;
    }

    public async Task<double> GetEmbodiedEmissionsAsync(IEnumerable<IComputeResource> resources, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var computeResources = new List<CloudComputeResource>();
        foreach (var r in resources)
        {
            var resource = r as CloudComputeResource ?? throw new ArgumentException("The compute resource is not of type CloudComputeResource", nameof(r));
            if(resource.UtilizationData != null)
            {
                resource = await GetVmUtilizationDataAsync(resource, periodStartTime, periodEndTime);
            }
            computeResources.Add(resource);
        }

        // 2000kgCO2 * 1000 = 2000000 gCO2
        var totalEmbodiedEmissions = 2000000;
        // 5 years == 43800 hours
        var expectedLifespanHours = 43800;

        double m = 0.0;
        foreach (var resource in computeResources)
        {
            foreach(var data in resource.UtilizationData)
            {
                var timeReserved = data.Duration.TotalHours;
                var timeShare = timeReserved / expectedLifespanHours;

                var resourceShare = (double)resource.VmSize.Cores / resource.VmSize.Processor.TotalCores;
                
                m += totalEmbodiedEmissions * timeShare * resourceShare;
            }
        }
        return m;
    }

    public async Task<double> GetEnergyAsync(IComputeResource computeResource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        /// https://docs.microsoft.com/en-us/dotnet/api/overview/azure/monitor/management?view=azure-dotnet
        /// https://stackoverflow.com/questions/54327418/get-cpu-utilization-of-virtual-machines-in-azure-using-python-sdk
        /// https://docs.microsoft.com/en-us/azure/virtual-machines/linux/metrics-vm-usage-rest
        /// https://github.com/Azure/azure-sdk-for-python/issues/9885
        /// https://docs.microsoft.com/en-us/samples/azure-samples/monitor-dotnet-metrics-api/monitor-dotnet-metrics-api/

        var resource = computeResource as CloudComputeResource ?? throw new ArgumentException("The compute resource is not of type CloudComputeResource", nameof(computeResource));

        resource = await GetVmUtilizationDataAsync(resource, periodStartTime, periodEndTime);

        return CalculationPowerConsumption(resource);
    }
    private async Task<CloudComputeResource> ResolveResourceAsync(IComputeResource resource)
    {
        var computeResource = new CloudComputeResource(){
            Name = resource.Name,
            Properties = resource.Properties
        };
        var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP_NAME");
        var vmName = resource.Name;
        var resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{vmName}";

        var credential = new DefaultAzureCredential();
        ArmClient armClient = new ArmClient(new DefaultAzureCredential());
        GenericResource vm = await armClient.GetGenericResource(new ResourceIdentifier(resourceId)).GetAsync();

        var location = vm.Data.Location.Name;
        computeResource.Location = new Location(){
            LocationType = LocationType.CloudProvider,
            CloudProvider = CloudProvider.Azure,
            RegionName = location
        };
        var props = vm.Data.Properties.ToObjectFromJson<VmProperties>();
        var vmSizeName = props.hardwareProfile.vmSize;
        var vmSizeDetails = this.VmSizeRepository[vmSizeName];

        if(!string.IsNullOrWhiteSpace(computeResource.ProcessorName))
        {
            vmSizeDetails.Processor = this.ProcessorRepository[computeResource.ProcessorName];
        }

        computeResource.VmSize = vmSizeDetails;

        return computeResource;
    }

    private async Task<CloudComputeResource> GetVmUtilizationDataAsync(CloudComputeResource computeResource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP_NAME");
        var vmName = computeResource.Name;
        var resourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{vmName}";

        var credential = new DefaultAzureCredential();
        var metricsQueryClient = new MetricsQueryClient(credential);

        var response = await metricsQueryClient.QueryResourceAsync(
            resourceId,
            metrics: new[] { "Percentage CPU" },
            options: new MetricsQueryOptions
            {
                Aggregations = {
                    MetricAggregationType.Average
                },
                TimeRange = new QueryTimeRange(
                    periodStartTime,
                    periodEndTime
                )
            });
        
        var duration = (TimeSpan)response.Value.Granularity;

        var utilizationData = new List<ComputeResourceUtilizationData>();

        foreach (var metric in response.Value.Metrics[0].TimeSeries[0].Values)
        {
            var cpu = metric.Average;
            var timestamp = metric.TimeStamp;

            if( cpu != null)
            {
                utilizationData.Add(new ComputeResourceUtilizationData()
                {
                    Timestamp = timestamp,
                    CpuUtilizationPercentage = (double)cpu / 100,
                    Duration = duration
                });
            }
        }

        computeResource.UtilizationData = utilizationData;

        return computeResource;
    }

    private double CalculationPowerConsumption(CloudComputeResource resource)
    {
        var allocatedCores = resource.VmSize.Cores;
        var totalCores = resource.VmSize.Processor.TotalCores;
        var idlePower = resource.VmSize.Processor.IdlePower;
        var maxPower = resource.VmSize.Processor.MaxPower;

        double result = 0.0;
        foreach (var data in resource.UtilizationData)
        {
            var powerDraw = ((data.CpuUtilizationPercentage * (maxPower - idlePower)) + idlePower) /1000; // KiloWatts
            var coreProportion = (double)allocatedCores / totalCores;
            result += (coreProportion * powerDraw * data.Duration.TotalHours);
        }
        return result;
    }

    private void initializeVmSizeRepository()
    {
        string name;
        name = "Intel(R) Xeon(R) Platinum 8272CL CPU @ 2.60GHz";
        this.ProcessorRepository.Add(name, new Processor(name, 26, 90.0, 200.0));
        name = "Intel(R) Xeon(R) Platinum 8370C CPU @ 2.60GHz";
        this.ProcessorRepository.Add(name, new Processor(name, 32, 85.0, 270.0));
        var edv4Processors = new List<Processor>()
        {
            this.ProcessorRepository["Intel(R) Xeon(R) Platinum 8272CL CPU @ 2.60GHz"],
            this.ProcessorRepository["Intel(R) Xeon(R) Platinum 8370C CPU @ 2.60GHz"]
        };
        var edv4Default = this.ProcessorRepository["Intel(R) Xeon(R) Platinum 8272CL CPU @ 2.60GHz"];

        this.VmSizeRepository.Add("Standard_E2d_v4", new VmSizeDetails("Standard_E2d_v4", 1, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E4d_v4", new VmSizeDetails("Standard_E4d_v4", 2, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E8d_v4", new VmSizeDetails("Standard_E8d_v4", 4, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E16d_v4", new VmSizeDetails("Standard_E16d_v4", 8, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E20d_v4", new VmSizeDetails("Standard_E20d_v4", 10, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E32d_v4", new VmSizeDetails("Standard_E32d_v4", 16, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E48d_v4", new VmSizeDetails("Standard_E48d_v4", 24, edv4Default, edv4Processors));
        this.VmSizeRepository.Add("Standard_E64d_v4", new VmSizeDetails("Standard_E64d_v4", 32, this.ProcessorRepository["Intel(R) Xeon(R) Platinum 8370C CPU @ 2.60GHz"], edv4Processors));
        // My Test VM
        this.VmSizeRepository.Add("Standard_DS1_v2", new VmSizeDetails("Standard_DS1_v2", 1, edv4Default, edv4Processors));

        System.Console.WriteLine("VmSizeRepository initialized");
        // Edsv4-series
        // { "Standard_E2ds_v4", 1, },
        // { "Standard_E4ds_v4",2, },
        // { "Standard_E8ds_v4",4, },
        // { "Standard_E16ds_v4",8, },
        // { "Standard_E20ds_v4",10, },
        // { "Standard_E32ds_v4",16, },
        // { "Standard_E48ds_v4",24, },
        // { "Standard_E64ds_v4",32, },
        // { "Standard_E80ids_v4",40, },
    }
}