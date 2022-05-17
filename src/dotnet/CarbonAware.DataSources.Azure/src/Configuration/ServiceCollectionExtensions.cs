using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Azure.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddAzureDataSourceService(this IServiceCollection services)
    {
        services.TryAddSingleton<IPowerConsumptionDataSource, AzureDataSource>();
    }
}