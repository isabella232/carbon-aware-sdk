namespace CarbonAware.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    private const string APPINSIGHTS_CONN_STR = "ApplicationInsights_Connection_String";
    private const string APPINSIGHTS_INST_KEY = "AppInsights_InstrumentationKey";

    public static void AddMonitoringAndTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = CreateConsoleLogger(configuration);
        var envVars = configuration?.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var telemetryProvider = GetTelemetryProviderFromValue(envVars?.TelemetryProvider);
        if (telemetryProvider != TelemetryProviderType.NotProvided)
        {
            logger.LogInformation($"Using Telemetry provider {telemetryProvider.ToString()}");
        }
        switch (telemetryProvider) {
            case TelemetryProviderType.ApplicationInsights:
            {
                if (IsAppInsightsConfigValid(logger, configuration)) {
                    services.AddApplicationInsightsTelemetry();
                }
                break;
            }
            case TelemetryProviderType.NotProvided:
            {
                break;
            }
          // Can be extended in the future to support a different provider like Zipkin, Prometheus etc 
        }

    }

    private static TelemetryProviderType GetTelemetryProviderFromValue(string? srcVal)
    {
        TelemetryProviderType result;
        if (String.IsNullOrEmpty(srcVal) ||
            !Enum.TryParse<TelemetryProviderType>(srcVal, true, out result))
        {
            return TelemetryProviderType.NotProvided;
        }
        return result;
    }

    private static ILogger CreateConsoleLogger(IConfiguration? config)
    {
        var factory = LoggerFactory.Create(b => {
            b.AddConfiguration(config?.GetSection("Logging"));
            b.AddConsole();
        });
        return factory.CreateLogger<IServiceCollection>();
    }

    private static Boolean IsAppInsightsConfigValid(ILogger logger, IConfiguration? config)
    {
        var isConnStrNull = String.IsNullOrEmpty(config?[APPINSIGHTS_CONN_STR]);
        var isInstKeyNull = String.IsNullOrEmpty(config?[APPINSIGHTS_INST_KEY]);
        if (isConnStrNull ^ isInstKeyNull) {
            logger.LogInformation($"{APPINSIGHTS_CONN_STR} or {APPINSIGHTS_INST_KEY} provided.");
            return true;
        }
        logger.LogWarning($"{APPINSIGHTS_CONN_STR} and {APPINSIGHTS_INST_KEY} not provided or both added");
        return false;
    }
}
