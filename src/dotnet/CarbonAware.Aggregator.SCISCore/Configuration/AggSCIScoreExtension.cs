using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.DepReg.Configuration;

namespace CarbonAware.Aggregator.SCISCore.Configuration;

public static class AggregatorSCIScoreServicesConfiguration
{
    public static void AddAggregatorSCIScoreServices(this IServiceCollection services)
    {
       services.TryAddSingleton<IAggregatorSCIScore, AggregatorSCIScore>();
       services.AddPluginService(PluginType.JSON);
       services.AddPluginService(PluginType.Energy);
    }
}