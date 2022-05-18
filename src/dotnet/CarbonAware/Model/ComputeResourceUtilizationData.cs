namespace CarbonAware.Model;

public class BaseComputeResource {
    public string Name { get; set; }

    public ComputeHardware Hardware { get; set; }

    public IEnumerable<ComputeResourceUtilizationData> UtilizationData { get; set; }
}

public class CloudComputeResource : BaseComputeResource
{
    public string CloudProvider { get; set; }
    public string VmType { get; set; }
}

public class ComputeResourceUtilizationData
{
    public DateTimeOffset Timestamp { get; set; }
    public double CpuUtilizationPercentage { get; set; } = 0.0;
    public TimeSpan Duration { get; set; }
}