namespace CarbonAware.Plugins.EnergyStatic;

using System.Threading.Tasks;
using CarbonAware;

public class EnergyConsumedStatic : IEnergyConsumed
{
    public async Task<int> GetConsumedAsync()
    {
        var val = 1;
        return await Task.Run(() => val);
    }
}
