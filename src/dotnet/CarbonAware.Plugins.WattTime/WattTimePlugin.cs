namespace CarbonAware.Plugins.WattTime;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware;
using CarbonAware.Model;

using CarbonAware.Tools.WattTimeClient;
public class WattTimePlugin : ICarbonAware
{
    IWattTimeClient _client;
    public WattTimePlugin(IWattTimeClient client)
    {
        _client = client;
    }

    public Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props)
    {
        throw new NotImplementedException();
    }
}
