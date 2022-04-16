using System.Collections;

namespace CarbonAware;

public interface IAggregatorSCIScore
{
    Task<int> CalcScore();
}