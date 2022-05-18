using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.SciScore;
using CarbonAware.DataSources.Configuration;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddDataSourceFactory(this IServiceCollection services)
    {
        services.TryAddSingleton<DataSourceFactory>();
    }
    /// <summary>
    /// Add services needed in order to pull data from a Carbon Intensity data source.
    /// </summary>
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
    }

    /// <summary>
    /// Add services needed in order to calculate SCI scores.
    /// </summary>
     public static void AddCarbonAwareSciScoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<ISciScoreAggregator, SciScoreAggregator>();
    }
}
