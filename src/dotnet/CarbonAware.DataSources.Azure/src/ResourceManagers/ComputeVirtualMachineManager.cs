
using CarbonAware.Interfaces;
using CarbonAware.DataSources.Azure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

namespace CarbonAware.DataSources.Azure.ResourceManagers;

public class ComputeVirtualMachineManager : IResourceManager
{
    public async Task<double> GetEnergyAsync(IComputeResource resource, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var vmResource = resource as CloudComputeResource ?? throw new ArgumentException("The compute resource is not of type CloudComputeResource", nameof(computeResource));

        vmResource = await GetVmUtilizationDataAsync(vmResource, periodStartTime, periodEndTime);

        return CalculationPowerConsumption(vmResource);
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
}
