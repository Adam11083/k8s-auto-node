namespace DemoApplication.Core.Services;

public interface IEc2Service
{
    Task<string> LaunchInstanceAsync(string instanceName);
    Task TerminateInstanceAsync(string instanceId);
    Task<string> GetInstancePublicIpAsync(string instanceId);
}