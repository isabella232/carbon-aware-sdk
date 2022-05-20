namespace CarbonAware.DataSources.Azure.Models;

/// <summary>
/// Represents the details of compute hardware required to compute or estimate energy consumption.
/// </summary>
public class VmSizeDetails
{
    // VM and hardware specs taken from https://docs.microsoft.com/en-us/azure/virtual-machines/edv4-edsv4-series
    // https://gadgetversus.com/processor/intel-xeon-platinum-8370c-vs-intel-xeon-platinum-8272cl/
    // https://medium.com/teads-engineering/estimating-aws-ec2-instances-power-consumption-c9745e347959
    // https://stackoverflow.com/questions/25835591/how-to-calculate-percentage-between-the-range-of-two-values-a-third-value-is
    
    private Processor _processor;

    public VmSizeDetails(string name, int cores, Processor defaultProcessor, List<Processor> validProcessors)
    {
        this.Name = name;
        this.Cores = cores;
        this._processor = defaultProcessor;
        this.ValidProcessors = validProcessors;
    }

    public string Name { get; }
    /// <summary>
    /// Number of cores utilized by the VM or total hardware cores for bare metal.
    /// </summary>
    public int Cores { get; }

    /// <summary>
    /// Processor options for this VM Family.
    /// </summary>
    public List<Processor> ValidProcessors { get; }

    public Processor Processor
    { 
        get
        {
            return _processor;
        }

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!this.ValidProcessors.Contains(value))
            {
                throw new ArgumentException($"{value.Name} is not a valid processor for {this.Name}");
            }

            _processor = value;
        }
    }
}

public class Processor
{
    public Processor(string name, int cores, double idlePower, double maxPower)
    {
        this.Name = name;
        this.TotalCores = cores; 
        this.IdlePower = idlePower;
        this.MaxPower = maxPower;
    }

    public string Name { get; }

    public int TotalCores { get; }

    public double IdlePower { get; }

    public double MaxPower { get; }
}