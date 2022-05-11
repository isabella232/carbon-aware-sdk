
using CarbonAware.CLI.Options;
using CommandLine;
using CommandLine.Text;

namespace CarbonAware.CLI;

using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.SciScore;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;

public class CarbonAwareCLI
{
    public CarbonAwareCLIState _state { get; set; } = new CarbonAwareCLIState();
    
    /// <summary>
    /// Indicates if the command line arguments have been parsed successfully 
    /// </summary>
    public bool Parsed { get; private set; } = false;
    ICarbonAwareAggregator carbonAwareAggregator {get; set;}

    ISciScoreAggregator sciScoreAggregator {get; set;}


    private readonly ILogger<CarbonAwareCLI> _logger;

    public CarbonAwareCLI(string[] args, ICarbonAwareAggregator caAggregator, ISciScoreAggregator sciScoreAggregator, ILogger<CarbonAwareCLI> logger)
    {
        this.carbonAwareAggregator = caAggregator;
        this.sciScoreAggregator = sciScoreAggregator;
        this._logger = logger;
        
        var parseResult = Parser.Default.ParseArguments<CLIOptions>(args);
        try
        {
            // Parse command line parameters
            parseResult.WithParsed(ValidateCommandLineArguments);
            parseResult.WithNotParsed(errors => ThrowOnParseError(errors, parseResult));
            Parsed = true;
        }
        catch (AggregateException e)
        {
            _logger.LogError("Error: {message}", e.Message);
        }
    }



    /// <summary>
    /// Handles missing messages.  Currently reports the message tag as an aggregate exception.
    /// This method needs updating to add detailed "Missing parameter" messages
    /// </summary>
    /// <param name="errors"></param>
    /// <exception cref="AggregateException"></exception>
    private void ThrowOnParseError(IEnumerable<Error> errors, ParserResult<CLIOptions> parseResult)
    {
        var builder = SentenceBuilder.Create();
        var errorMessages = HelpText.RenderParsingErrorsTextAsLines(parseResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);
        var excList = errorMessages.Select(msg => new ArgumentException(msg)).ToList();
        if (excList.Any())
        {
            throw new AggregateException(excList);
        }
    }

    public async Task GetCarbonEmissionsData()
    {
        if(!this.Parsed) 
        {
            return;
        }
        switch(_state.Route) 
        {
            
            case RouteOptions.EmissionsForLocationsByTime: 
            {
                var result = await GetEmissions(false);
                OutputEmissionsData(result);
                break;
            } 
            case RouteOptions.BestEmissionsForLocationsByTime: 
            {
                var result = await GetEmissions(true);
                OutputEmissionsData(result);
                break;
            }
            case RouteOptions.SciScore: 
            {
                var result = await GetSciScore();
                OutputEmissionsData(result);
                break;
            }
        }
        
        
        // IEnumerable<Location> locations = _state.Locations.Select(loc => new Location(){ RegionName = loc });
        // var props = new Dictionary<string, object>() {
        //     { CarbonAwareConstants.Locations, locations },
        //     { CarbonAwareConstants.Start, _state.Time },
        //     { CarbonAwareConstants.End, _state.ToTime },
        //     { CarbonAwareConstants.Best, true }
        // };
        // return await GetEmissionsDataAsync(props);
    }

    private async Task<double> GetSciScore()
    {
        IEnumerable<Location> locations = _state.Locations.Select(loc => new Location(){ RegionName = loc });
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, locations },
            { CarbonAwareConstants.Start, _state.Time },
            { CarbonAwareConstants.End, _state.ToTime },
            { CarbonAwareConstants.Best, true }
        };

        //return await sciScoreAggregator.CalculateAverageCarbonIntensityAsync(_state.Locations, );
        return 1.00;
    }

    //TODO: Add Method documentation
    public async Task<IEnumerable<EmissionsData>> GetEmissions(bool isBest)
    {
        IEnumerable<Location> locations = _state.Locations.Select(loc => new Location(){ RegionName = loc });
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, locations },
            { CarbonAwareConstants.Start, _state.Time },
            { CarbonAwareConstants.End, _state.ToTime },
            { CarbonAwareConstants.Best, isBest }
        };
        return await GetEmissionsDataAsync(props);
    }

    private async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(Dictionary<string, object> props)
    {
        IEnumerable<EmissionsData> e = await carbonAwareAggregator.GetEmissionsDataAsync(props);

        return await carbonAwareAggregator.GetEmissionsDataAsync(props);
    }

    public void OutputEmissionsData(object emissions)
    {
        var outputData = $"{JsonConvert.SerializeObject(emissions, Formatting.Indented)}";
        _logger.LogDebug(outputData);
        Console.WriteLine(outputData);
    }

    private void ValidateCommandLineArguments(CLIOptions o)
    {
        // -v --verbose 
        ParseVerbose(o);

        // -t --time --toTime
        ParseTime(o);

        // --lowest
        ParseLowest(o);

        // -c --config
        ParseConfigPath(o);

        // -l --locations
        ParseLocations(o);

        // -r --api route
        ParseRoute(o);
    }

    private void ParseRoute(CLIOptions o)
    {
        _state.Route = o.Route;
    }

    #region Parse Options 

    private void ParseLocations(CLIOptions o)
    {

        _state.Locations.AddRange(o.Location);
    }

    private void ParseLowest(CLIOptions o)
    {
        _state.Lowest = o.Lowest;
    }

    private void ParseVerbose(CLIOptions o)
    {
        if (o.Verbose)
        {
            _state.Verbose = true;
        }
    }

    private void ParseConfigPath(CLIOptions o)
    {
        var configPath = o.ConfigPath;

        if (configPath is not null)
        {
            CheckFileExists(configPath);
            _state.ConfigPath = configPath;
        }
    }

    private static void CheckFileExists(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new ArgumentException($"File '{configPath}' could not be found.");
        }
    }

    private void ParseTime(CLIOptions o)
    {
        ParseTimeFromTime(o);
        ParseTimeToTime(o);
    }

    private void ParseTimeFromTime(CLIOptions o)
    {
        if (o.Time is not null)
        {
            _state.TimeOption = TimeOptionStates.Time;
            try
            {
                _state.Time = DateTime.Parse(o.Time);
            }
            catch
            {
                throw new ArgumentException(
                    $"Date and time needs to be in the format 'xxxx-xx-xx'.  Date and time provided was '{o.Time}'.");
            }
        }
    }

    private void ParseTimeToTime(CLIOptions o)
    {
        if (o.ToTime is not null)
        {
            _state.TimeOption = TimeOptionStates.TimeWindow;

            try
            {
                _state.ToTime = DateTime.Parse(o.ToTime);
            }
            catch
            {
                throw new ArgumentException(
                    $"Date and time needs to be in the format 'xxxxx'.  Date and time provided was '{o.ToTime}'.");
            }
        }
    }

    #endregion
}
