using CarbonAware.Model;
using SciScoreModel = CarbonAware.Model.SciScore;
using CarbonAware.Interfaces;
using CarbonAware.DataSources.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

//TODO(bderusha) Remove this after resolver is abstracted
using CarbonAware.DataSources.Azure;

namespace CarbonAware.Aggregators.SciScore;

/// <summary>
/// An aggregator to service SCI Score operations.
/// </summary>
public class SciScoreAggregator : ISciScoreAggregator
{
    private readonly ILogger<SciScoreAggregator> _logger;

    private readonly ICarbonIntensityDataSource _carbonIntensityDataSource;

    private readonly IPowerConsumptionDataSource _energyDataSource;

    // private readonly DataSourceFactory _dataSourceFactory;

    /// <summary>
    /// Creates a new instance of the <see cref="SciScoreAggregator"/> class.
    /// </summary>
    /// <param name="logger">A logger for the SCI Score aggregator.</param>
    /// <param name="carbonIntensityDataSource">An <see cref="ICarbonIntensityDataSource"> data source.</param>
    /// <exception cref="ArgumentNullException">Can be thrown if no logger is provided.</exception>
    public SciScoreAggregator(ILogger<SciScoreAggregator> logger, ICarbonIntensityDataSource carbonIntensityDataSource, IPowerConsumptionDataSource energyDataSource)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._carbonIntensityDataSource = carbonIntensityDataSource;
        this._energyDataSource = energyDataSource;
        // this._dataSourceFactory = dataSourceFactory;
    }

    public async Task<SciScoreModel> CalculateSciScoreAsync(IEnumerable<IComputeResource> computeResources, string timeInterval)
    {
        var resources = await resolveResources(computeResources);
        double totalCI = 0.0;
        double totalEnergy = 0.0;
        double embodiedEmissions = 0.0;
        int functionalUnit = 1;
        foreach (var resource in resources)
        {
            totalCI += await CalculateAverageCarbonIntensityAsync(resource.Location, timeInterval);
        }
        totalEnergy = await CalculateEnergyAsync(resources, timeInterval);
        embodiedEmissions = await CalculateEmbodiedEmissionsAsync(resources, timeInterval);
        
        var score = new SciScoreModel()
        {
            MarginalCarbonIntensityValue = totalCI,
            EnergyValue = totalEnergy,
            EmbodiedEmissionsValue = embodiedEmissions,
            FunctionalUnitValue = functionalUnit,
            SciScoreValue = ((totalEnergy * totalCI) + embodiedEmissions)/functionalUnit
        };

        return score;
    }

    /// <inheritdoc />
    public async Task<double> CalculateAverageCarbonIntensityAsync(Location location, string timeInterval)
    {
        (DateTimeOffset start, DateTimeOffset end) = this.ParseTimeInterval(timeInterval);
        var emissionData = await this._carbonIntensityDataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);

        // check whether the emissionData list is empty, if not, return Rating's average, otherwise 0.
        var value = emissionData.Any() ? emissionData.Select(x => x.Rating).Average() : 0;
        _logger.LogInformation($"Carbon Intensity Average: {value}");

        return value;
    }

    /// <inheritdoc />
    public async Task<double> CalculateEnergyAsync(IEnumerable<IComputeResource> computeResources, string timeInterval)
    {
        (DateTimeOffset start, DateTimeOffset end) = this.ParseTimeInterval(timeInterval);

        // var dataSource = this._dataSourceFactory.GetEnergyDataSource(DataSourceType.Azure);

        var value = 0.0;
        foreach (var computeResource in computeResources)
        {
            value += await this._energyDataSource.GetEnergyAsync(computeResource, start, end);
        }
        
        return value;
    }

    private async Task<double> CalculateEmbodiedEmissionsAsync(IEnumerable<IComputeResource> computeResources, string timeInterval)
    {
        (DateTimeOffset start, DateTimeOffset end) = this.ParseTimeInterval(timeInterval);
        /// TODO(bderusha) Abstract this into a new datasource interface
        var embodiedEmissionsDataSource = new AzureDataSource();
        return await embodiedEmissionsDataSource.GetEmbodiedEmissionsAsync(computeResources, start, end);
    }

    private async Task<IEnumerable<IComputeResource>> resolveResources(IEnumerable<IComputeResource> computeResources)
    {
        /// TODO(bderusha) Abstract this into a new datasource interface
        var resolver = new AzureDataSource();
        return await resolver.ResolveResourcesAsync(computeResources);
    }

    // Validate and parse time interval string into a tuple of (start, end) DateTimeOffsets.
    // Throws ArgumentException for invalid input.
    private (DateTimeOffset start, DateTimeOffset end) ParseTimeInterval(string timeInterval)
    {
        DateTimeOffset start;
        DateTimeOffset end;

        var timeIntervals = timeInterval.Split('/');
        // Check that the time interval was split into exactly 2 parts
        if(timeIntervals.Length != 2)
        {
            throw new ArgumentException(
                $"Invalid TimeInterval. Expected exactly 2 dates separated by '/', recieved: {timeInterval}"
            );
        }

        var rawStart = timeIntervals[0];
        var rawEnd = timeIntervals[1];

        if(!DateTimeOffset.TryParse(rawStart, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out start))
        {
            throw new ArgumentException($"Invalid TimeInterval. Could not parse start time: {rawStart}");
        }

        if(!DateTimeOffset.TryParse(rawEnd, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out end))
        {
            throw new ArgumentException($"Invalid TimeInterval. Could not parse end time: {rawEnd}");
        }

        if(start > end)
        {
            throw new ArgumentException($"Invalid TimeInterval. Start time must come before end time: {timeInterval}");
        }

        return (start, end);
    }

}