namespace CarbonAwareCLI.Options;

/// <summary>
/// List of API Routes.
/// </summary>
public enum RouteOptions
{
    // Get best Carbon emissions data for the provided location and time.
    BestEmissionsForLocationsByTime,

    // Get carbon emissions data for the provided location and time.
    EmissionsForLocationsByTime,

    // Get average marginal carbon intensity.
    MarginalCarbonIntensity,

    // Get SciScore
    SciScore
}

/// <summary>
/// The cloud provider type for the location.
/// </summary>
public enum CloudProvider
{
    // Azure location.  Region is expected to be set to an Azure region name.
    Azure,

    // Aws location.  Region is expected to be set to an AWS region.
    AWS
}