using DemoApplication.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NodeController(
    IEc2Service ec2Service,
    ILogger<NodeController> logger)
    : ControllerBase
{
    // GET: api/node
    [HttpGet]
    [Authorize]
    public Task GetAllNodes()
    {
        logger.Log(LogLevel.Information, "Get All Nodes");
        return Task.CompletedTask;
    }

    [HttpPost]
    // [Authorize]
    public async Task AddNode()
    {
        await ec2Service.LaunchInstanceAsync("test-auto-node-3");
    }
}