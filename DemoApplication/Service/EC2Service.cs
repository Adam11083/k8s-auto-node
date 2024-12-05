using Amazon.EC2;
using Amazon.EC2.Model;
using DemoApplication.Core.Constants;
using DemoApplication.Core.Services;

namespace DemoApplication.Service;

public class Ec2Service(IAmazonEC2 ec2Client) : IEc2Service
{
    public async Task<string> LaunchInstanceAsync(string instanceName)
    {
        var tagSpecification = new TagSpecification
        {
            ResourceType = ResourceType.Instance,
            Tags = [new Tag { Key = "Name", Value = instanceName }]
        };

        var runRequest = new RunInstancesRequest
        {
            ImageId = Ec2LaunchConstant.ImageId,
            InstanceType = Ec2LaunchConstant.InstanceType,
            MinCount = 1,
            MaxCount = 1,
            KeyName = Ec2LaunchConstant.KeyName,
            SecurityGroupIds = [Ec2LaunchConstant.SecurityGroupIds],
            TagSpecifications = [tagSpecification],
            UserData = Ec2LaunchConstant.Base64WorkerNodeScript
        };

        var response = await ec2Client.RunInstancesAsync(runRequest);

        var instanceId = response.Reservation.Instances[0].InstanceId;
        Console.WriteLine($"  New instance: {instanceId}");
        // Wait for the instance to be running

        return instanceId;
    }

    public async Task<string> GetInstancePublicIpAsync(string instanceId)
    {
        var request = new DescribeInstancesRequest
        {
            InstanceIds = [instanceId]
        };

        var response = await ec2Client.DescribeInstancesAsync(request);
        return response.Reservations[0].Instances[0].PublicIpAddress;
    }

    public async Task TerminateInstanceAsync(string instanceId)
    {
        var terminateRequest = new TerminateInstancesRequest { InstanceIds = [instanceId] };
        await ec2Client.TerminateInstancesAsync(terminateRequest);
    }

    //
    // Method to wait until the instances are running (or at least not pending)
    private static async Task CheckState(IAmazonEC2 ec2Client, List<string> instanceIds)
    {
        Console.WriteLine(
            "\nWaiting for the instances to start." +
            "\nPress any key to stop waiting. (Response might be slightly delayed.)");

        int numberRunning;
        DescribeInstancesResponse responseDescribe;
        var requestDescribe = new DescribeInstancesRequest
        {
            InstanceIds = instanceIds
        };

        // Check every couple of seconds
        var wait = 2000;
        while (true)
        {
            // Get and check the status for each of the instances to see if it's past pending.
            // Once all instances are past pending, break out.
            // (For this example, we are assuming that there is only one reservation.)
            Console.Write(".");
            numberRunning = 0;
            responseDescribe = await ec2Client.DescribeInstancesAsync(requestDescribe);
            foreach (var i in responseDescribe.Reservations[0].Instances)
                // Check the lower byte of State.Code property
                // Code == 0 is the pending state
                if ((i.State.Code & 255) > 0)
                    numberRunning++;
            if (numberRunning == responseDescribe.Reservations[0].Instances.Count)
                break;

            // Wait a bit and try again (unless the user wants to stop waiting)
            Thread.Sleep(wait);
            if (Console.KeyAvailable)
                break;
        }

        Console.WriteLine("\nNo more instances are pending.");
        foreach (var i in responseDescribe.Reservations[0].Instances)
        {
            Console.WriteLine($"For {i.InstanceId}:");
            Console.WriteLine($"  VPC ID: {i.VpcId}");
            Console.WriteLine($"  Instance state: {i.State.Name}");
            Console.WriteLine($"  Public IP address: {i.PublicIpAddress}");
            Console.WriteLine($"  Public DNS name: {i.PublicDnsName}");
            Console.WriteLine($"  Key pair name: {i.KeyName}");
        }
    }
}