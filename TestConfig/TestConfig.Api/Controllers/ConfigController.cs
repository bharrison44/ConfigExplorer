using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TestConfig.Api.Controllers;

/// <summary>
/// API controller that uses configuration values.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IOptionsSnapshot<ExampleConfigModel> _ConfigBinding;

    /// <summary>
    /// Constructor for this controller.
    /// </summary>
    /// <param name="configBinding">The bound configuration model.</param>
    public ConfigController(IOptionsSnapshot<ExampleConfigModel> configBinding)
    {
        _ConfigBinding = configBinding;
    }
    
    /// <summary>
    /// Get the configuration values.
    /// </summary>
    [HttpGet(Name = "GetConfigValue")]
    public IActionResult Get()
    {
        var nonSecret = _ConfigBinding.Value.NonSecret;
        var verySecret = _ConfigBinding.Value.KvSecret;
        
        return Ok($"{nonSecret} -- {verySecret}");
    }
}