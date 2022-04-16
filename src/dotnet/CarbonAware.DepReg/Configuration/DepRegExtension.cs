namespace CarbonAware.DepReg.Configuration;

using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using CarbonAware.Plugins.WattTime.Configuration;
using CarbonAware.Plugins.EnergyStatic.Configuration;


public static class DepRegServiceExtension
{
    public static void AddPluginService(this IServiceCollection services, PluginType pType)
    {
        switch (pType)
        {
                case PluginType.JSON:
                {
                    services.AddCarbonAwareServices();
                    break;
                }
                case PluginType.WattTime:
                {
                    services.AddWattTimeServices();
                    break;
                }
                case PluginType.Energy:
                {
                    services.AddEneryStaticServices();
                    break;
                }
        }
    }

}

public enum PluginType
{
    None,
    WattTime,
    JSON,
    Energy
}
