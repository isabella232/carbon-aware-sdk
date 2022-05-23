
using CarbonAware.DataSources.Azure.Models;
using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Azure.ResourceManagers;
public interface IResourceManager
{
    public Task<double> GetEnergyAsync(IComputeResource resource, DateTimeOffset startTime, DateTimeOffset endTime);
}