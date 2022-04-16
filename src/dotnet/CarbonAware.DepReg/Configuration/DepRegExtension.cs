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
                case PluginType.CarbonIntesity_Json:
                {
                    services.AddCarbonAwareServices();
                    break;
                }
                case PluginType.CarbonIntesity_WattTime:
                {
                    services.AddWattTimeServices();
                    break;
                }
                case PluginType.Energy_Static:
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
    CarbonIntesity_WattTime,
    CarbonIntesity_Json,
    Energy_Static
}
