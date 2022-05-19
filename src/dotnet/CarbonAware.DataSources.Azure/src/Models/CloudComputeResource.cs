using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Azure.Models;

public class CloudComputeResource : IComputeResource
{
    public string Name { get; set; }

    public VmSizeDetails VmSize { get; set; }

    public IEnumerable<ComputeResourceUtilizationData> UtilizationData { get; set; }

    public string RegionName { get; set; }

    public string? ProcessorName {
        get => Properties.GetValueOrDefault("processor");
    }

    public Dictionary<string, string> Properties { get; set; }
}

public class ComputeResourceUtilizationData
{
    public DateTimeOffset Timestamp { get; set; }
    public double CpuUtilizationPercentage { get; set; } = 0.0;
    public TimeSpan Duration { get; set; }
}

public class VmProperties
{
    public HardwareProfile hardwareProfile { get; set; }
}

public class HardwareProfile
{
    public string vmSize { get; set; }
}