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
    .Configure<IConfiguration>((options, config) => config.Bind("SendGrid", options));

builder
    .Configuration
    .AddAzureAppConfiguration(options =>
    {
        options.Connect("AddTheConfigExplorerConnectionHere")
            .UseFeatureFlags()
            .ConfigureRefresh(refresh =>
            {
                refresh
                    .Register("SentinelKey--FunctionApp", refreshAll: true)
                    .SetCacheExpiration(TimeSpan.FromMinutes(5));
            })
            .Select("FunctionApp--*")
            .TrimKeyPrefix("FunctionApp--")
            .ConfigureKeyVault(config => { config.SetCredential(new DefaultAzureCredential()); });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();