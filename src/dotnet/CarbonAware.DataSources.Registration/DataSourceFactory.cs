using CarbonAware.Interfaces;
using CarbonAware.DataSources.Configuration;
using CarbonAware.DataSources.Json;
using CarbonAware.DataSources.WattTime;
using CarbonAware.DataSources.Azure;

namespace CarbonAware.DataSources.Registration;

public class DataSourceFactory
{
    private readonly IServiceProvider serviceProvider;

    public DataSourceFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public ICarbonIntensityDataSource GetCarbonIntensityDataSource(DataSourceType dataSourceType)
    {
        object dataSource;
        switch (dataSourceType)
        {
            case DataSourceType.JSON:
                dataSource = serviceProvider.GetService(typeof(JsonDataSource)) ?? throw new NotSupportedException();
                break;
            case DataSourceType.WattTime:
                dataSource = serviceProvider.GetService(typeof(WattTimeDataSource)) ?? throw new NotSupportedException();
                break;
            default:
                throw new NotSupportedException($"DataSourceType {dataSourceType.ToString()} not supported");
        }

        return (ICarbonIntensityDataSource)dataSource;
    }

    public IPowerConsumptionDataSource GetEnergyDataSource(DataSourceType dataSourceType)
    {
        object dataSource;
        switch (dataSourceType)
        {
            case DataSourceType.Azure:
                dataSource = serviceProvider.GetService(typeof(AzureDataSource)) ?? throw new NotSupportedException();
                break;
            default:
                throw new NotSupportedException($"DataSourceType {dataSourceType.ToString()} not supported");
        }

        return (IPowerConsumptionDataSource)dataSource;
    }
}