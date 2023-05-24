using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TestConfig.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IOptionsSnapshot<SendGridConfigModel> _configuration;

    public ConfigController(IOptionsSnapshot<SendGridConfigModel> configuration)
    {
        _configuration = configuration;
    }

    [HttpGet(Name = "GetConfigValue")]
    public IActionResult Get()
    {
        var apiKey = _configuration.Value.ApiKey;
        var apiUri = _configuration.Value.ApiUri;

        return Ok($"ApiKey: {apiKey} and the ApiUri: {apiUri}");
    }
}