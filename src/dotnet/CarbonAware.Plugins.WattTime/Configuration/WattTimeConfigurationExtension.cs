using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Plugins.WattTime.Configuration;

public static class WattTimeServicesConfiguration
{
    public static void AddWattTimeServices(this IServiceCollection services)
    {
        services.ConfigureWattTimeClient(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
        services.TryAddSingleton<ICarbonAware, WattTimePlugin>();
    }
}