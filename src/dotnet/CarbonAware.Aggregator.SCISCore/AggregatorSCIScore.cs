namespace CarbonAware.Aggregator.SCISCore;

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class AggregatorSCIScore : IAggregatorSCIScore
{
    private readonly IEnergyConsumed _energy;
    private readonly ICarbonAware _carbon;
    private ILogger<AggregatorSCIScore> _logger;

    public AggregatorSCIScore(ILogger<AggregatorSCIScore> logger, IEnergyConsumed energy, ICarbonAware carbon)
    {
        _logger = logger;
        _energy = energy;
        _carbon = carbon;
    }

    public async Task<int> CalcScore()
    {
        var v1 = await _energy.GetConsumedAsync() + 10;
        var e = await _carbon.GetEmissionsDataAsync(new Dictionary<string, object>());
        foreach (var ee in e)
        {
            _logger.LogInformation($"{ee.Location}");
        }
        return await Task.Run(() => v1);
    }
}
