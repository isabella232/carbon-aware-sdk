using System.Diagnostics;

namespace CarbonAware.WebApi.Configuration;

public static class TelemetryActivity
{
    public const string ServiceName = "CarbonAwareService";
    public static readonly ActivitySource Activity = new(ServiceName);
}