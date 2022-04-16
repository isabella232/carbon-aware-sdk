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
        _logger.LogInformation("Calculating Score");
        var v1 = await _energy.GetConsumedAsync() + 10;
        var elems = await _carbon.GetEmissionsDataAsync(new Dictionary<string, object>());
        var tot = v1 + elems.Where(x => x.Rating < 20).Count();
        _logger.LogInformation($"Calculating Score Done: {tot}");
        return await Task.Run(() => tot);
    }
}
