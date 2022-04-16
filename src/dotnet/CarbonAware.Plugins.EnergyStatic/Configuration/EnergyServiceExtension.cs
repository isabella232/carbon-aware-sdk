using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Plugins.EnergyStatic.Configuration;
public static class EnergyStaticServicesConfiguration
{
    public static void AddEneryStaticServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IEnergyConsumed, EnergyConsumedStatic>();
    }
}