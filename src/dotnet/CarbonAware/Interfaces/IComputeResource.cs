namespace CarbonAware.Interfaces;

public interface IComputeResource {
    public string Name { get; set; }

    public Location Location { get; set; }

    public Dictionary<string, string> Properties { get; set; }
}