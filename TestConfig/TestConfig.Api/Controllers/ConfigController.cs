using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TestConfig.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IOptionsSnapshot<SendGridConfigModel> _configuration;
    private readonly IConfiguration _RawConfiguration;

    public ConfigController(IOptionsSnapshot<SendGridConfigModel> configuration, IConfiguration rawConfiguration)
    {
        _configuration = configuration;
        _RawConfiguration = rawConfiguration;
    }

    [HttpGet(Name = "GetConfigValue")]
    public IActionResult Get()
    {
        var nonsecret = _configuration.Value.Nonsecret;
        var verysecret = _configuration.Value.Verysecret;
        var version = _configuration.Value.Sentinel;

        var rawNonsecret = _RawConfiguration.GetSection("service1")["nonsecret"];
        var rawVerysecret = _RawConfiguration.GetSection("service1")["verysecret"];
        var rawVersion = _RawConfiguration.GetSection("service1")["sentinel"];

        return Ok($"{version} -- {nonsecret} -- {verysecret} //// {rawVersion} - {rawNonsecret} - {rawVerysecret}");
    }
}