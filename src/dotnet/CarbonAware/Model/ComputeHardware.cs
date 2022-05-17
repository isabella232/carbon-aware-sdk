namespace CarbonAware.Model;

/// <summary>
/// Represents the details of compute hardware required to compute or estimate energy consumption.
/// </summary>
public class ComputeHardware
{
    // VM and hardware specs taken from https://docs.microsoft.com/en-us/azure/virtual-machines/edv4-edsv4-series
    // The two processors for this class of VM have different power consumption rates and total cores.
    // PowerPerCore = ("8370C TPD" + "8272CL TPD")/("8370C Cores" + "8272CL Cores") OR (270 + 195)/(32 + 26)
    // https://gadgetversus.com/processor/intel-xeon-platinum-8370c-vs-intel-xeon-platinum-8272cl/
    private readonly Dictionary<string, (int Cores, double PowerPerCore)> _namedComputes = new Dictionary<string, (int Cores, double PowerPerCore)>()
    {
        // Edv4-series
        { "Azure.Standard_E2d_v4", (1, 8.01724) },
        { "Azure.Standard_E4d_v4", (2, 8.01724) },
        { "Azure.Standard_E8d_v4", (4, 8.01724) },
        { "Azure.Standard_E16d_v4", (8, 8.01724) },
        { "Azure.Standard_E20d_v4", (10, 8.01724) },
        { "Azure.Standard_E32d_v4", (16, 8.01724) },
        { "Azure.Standard_E48d_v4", (24, 8.01724) },
        { "Azure.Standard_E64d_v4", (32, 8.01724) },
        // Edsv4-series
        { "Azure.Standard_E2ds_v4", (1, 8.01724) },
        { "Azure.Standard_E4ds_v4", (2, 8.01724) },
        { "Azure.Standard_E8ds_v4", (4, 8.01724) },
        { "Azure.Standard_E16ds_v4", (8, 8.01724) },
        { "Azure.Standard_E20ds_v4", (10, 8.01724) },
        { "Azure.Standard_E32ds_v4", (16, 8.01724) },
        { "Azure.Standard_E48ds_v4", (24, 8.01724) },
        { "Azure.Standard_E64ds_v4", (32, 8.01724) },
        { "Azure.Standard_E80ids_v4", (40, 8.01724) },
    };

    public ComputeHardware(string namedCompute)
    {
        (this.Cores, this.PowerPerCore) = _namedComputes[namedCompute];
    }

    public ComputeHardware(int cores, double powerPerCore)
    {
        this.Cores = cores;
        this.PowerPerCore = powerPerCore;
    }

    /// <summary>
    /// Number of cores utilized by the VM or total hardware cores for bare metal.
    /// </summary>
    public int Cores { get; }

    /// <summary>
    /// Power consumption of the CPU per core.
    /// </summary>
    public double PowerPerCore { get; }
}