using System.Collections;

namespace CarbonAware;

public interface IEnergyConsumed
{
    Task<int> GetConsumedAsync();
}