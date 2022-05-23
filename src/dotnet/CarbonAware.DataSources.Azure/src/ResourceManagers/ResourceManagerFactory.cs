namespace CarbonAware.DataSources.Azure.ResourceManagers;

public class ResourceManagerFactory
{
    public ResourceManagerFactory(){ }
    
    public IResourceManager Create(string resourceType)
    {
        switch (resourceType)
        {
            case "Microsoft.Compute/virtualMachines":
                return new ComputeVirtualMachineManager();
            default:
                throw new NotSupportedException($"ResourceType {resourceType} not supported");
        }
    }
}