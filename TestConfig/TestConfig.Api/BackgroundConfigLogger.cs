using Microsoft.Extensions.Options;

namespace TestConfig.Api;

/// <summary>
/// Background service that logs configuration
/// </summary>
public class BackgroundConfigLogger : BackgroundService
{
    private readonly IConfiguration _Configuration;
    private readonly IServiceProvider _ServiceProvider;
    private readonly ILogger<BackgroundConfigLogger> _Logger;

    /// <summary>
    /// Constructor for this service.
    /// </summary>
    /// <param name="configuration">.NETs configuration object.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public BackgroundConfigLogger(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<BackgroundConfigLogger> logger)
    {
        _Configuration = configuration;
        _ServiceProvider = serviceProvider;
        _Logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogFromConfigObj();
            LogFromBinding();

            await Task.Delay(1000, stoppingToken);
        }
    }

    private void LogFromConfigObj()
    {
        var section = _Configuration.GetSection("Example");

        var nonSecret = section["NonSecret"];
        var verySecret = section["KvSecret"];

        _Logger.LogInformation($"[{DateTime.UtcNow.TimeOfDay}] Config Obj: {nonSecret} -- {verySecret}");
    }

    private void LogFromBinding()
    {
        using var scope = _ServiceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ExampleConfigModel>>();

        _Logger.LogInformation($"[{DateTime.UtcNow.TimeOfDay}] Binding: {config.Value.NonSecret} -- {config.Value.KvSecret}");
    }
}