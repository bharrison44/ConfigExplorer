using Microsoft.Extensions.Options;
using System;

namespace TestConfig.Api
{
    public class BackgroundConfigLogger : BackgroundService
    {
        private readonly IConfiguration _CtorConfiguration;
        private readonly IServiceProvider _ServiceProvider;
        private readonly ILogger<BackgroundConfigLogger> _Logger;

        public BackgroundConfigLogger(
            IConfiguration ctorConfiguration,
            IServiceProvider serviceProvider,
            ILogger<BackgroundConfigLogger> logger)
        {
            _CtorConfiguration = ctorConfiguration;
            _ServiceProvider = serviceProvider;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //using var scope = _ServiceProvider.CreateScope();
                //var config = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SendGridConfigModel>>();
                var localConfiguration = _ServiceProvider.GetRequiredService<IConfiguration>();

                Log("Local", localConfiguration);
                Log("Ctor", _CtorConfiguration);

                await Task.Delay(1000);
            }
        }

        private void Log(string label, IConfiguration configuration)
        {
            var section = configuration.GetSection("service1");

            var nonsecret = section["nonsecret"];
            var verysecret = section["verysecret"];

            _Logger.LogInformation($"[{DateTime.UtcNow.TimeOfDay}] {label}: {nonsecret} -- {verysecret}");

        }
    }
}
