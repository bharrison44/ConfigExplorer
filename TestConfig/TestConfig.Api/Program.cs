using Azure.Identity;
using TestConfig.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<SendGridConfigModel>()
    .Configure<IConfiguration>((options, config) => config.Bind("service1", options));

var appConfigConnectionString = builder.Configuration.GetConnectionString("appConfig");

builder.Services.AddHostedService<BackgroundConfigLogger>();

builder.Services.AddAzureAppConfiguration();
builder
    .Configuration
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(appConfigConnectionString)
            .UseFeatureFlags()
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register("config-version", refreshAll: true);
            })
            .Select("test--*")
            .TrimKeyPrefix("test--")
            .ConfigureKeyVault(config => config.SetCredential(new DefaultAzureCredential()));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAzureAppConfiguration();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();