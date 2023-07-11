# ConfigExplorer

POC to setup config explorer with AddAzureAppConfiguration

## Example Project Setup

* copy the `appsettings.example.json` file to `appsettings.json`
* populate the `appConfig` connection string with the value from the Azure App Configuration portal `Access Keys` section
* setup a KeyVault instance with a secret - the name and contents is not important
* setup an App Configuration instance with
    * a key-value configuration item with the name `config-version` and a value of `0`
    * a key-value configuration item with the name `test--service1:nonsecret` and any non-empty value
    * a KeyVault reference configuration item for the KeyVault secret with the name `test--service1:verysecret`

## App Configuration Pattern Recommendation

### Azure App Configuration Setup

* add a iteration tracking config value to the App Configuration instance
    * can be named and contain anything, but recommended is `config-version` containing an integer that gets incremented when config is updated
* add configuration values named in the following format `{prefix}--{section}:{item}`
    * prefix is optional, but recommended
    * can be key-value or KeyVault reference - it makes no difference

### App Setup

* add package `Microsoft.Azure.AppConfiguration.AspNetCore`
* add service registration to `Program.cs`/`Setup.cs`

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureAppConfiguration();
```

* load App Configuration connection string from local configuration (`appsettings.json`, environment variable, etc)
    * if using `appsettings.json` or another file-based method, ensure this file is added to the repositories `.gitignore` file

* (optional) bind configuration model/s

```c#
builder.Services
    .AddOptions<ExampleConfigModel>()
    .Configure<IConfiguration>((options, config) => config.Bind("exampleSection", options));
```

```c#
var appConfigConnectionString = builder.Configuration.GetConnectionString("appConfig");
```

* add config setup to `Program.cs`/`Setup.cs`
    * chain call `Select` and `TrimKeyPrefix` using the configuration prefix (ie: `example--`) if one is used by the configuration keys
    * add `.UseFeatureFlag()` to `AddAzureAppConfiguration` `options` call chain if needed
    * chain call `.SetCacheExpiration(Timespan)` after `.Register` to specify a different refresh interval - default is 30s
    * use alternative Azure credentials, if needed

```c#
builder
    .Configuration
    .AddAzureAppConfiguration(options =>
    {
        options.Connect(appConfigConnectionString)
            .ConfigureRefresh(refresh =>
            {
                refresh.Register("config-version", refreshAll: true);
            })
            .Select("example--*")
            .TrimKeyPrefix("example--")
            .ConfigureKeyVault(config => config.SetCredential(new DefaultAzureCredential()));
    });
```

* add service initialisation to `Program.cs`/`Setup.cs`

```c#
var app = builder.Build();

app.UseAzureAppConfiguration();
```

### Accessing Configuration Values

* use `IConfiguration` directly, or `IOptionsSnapshot<T>` if bound to a config model via dependency injected resources
* do not use `IOption<T>` or `IOptionMonitor<T>` - these don't update and will always contain initial values
* in singleton resources, background services, etc `IOptionSnapshot<T>` can be used, but as it is a scoped resource it cannot be used as a constructor parameter. instead accept `IServiceProvider` from the constructor and open a scope when needed.

```c#
using var scope = _ServiceProvider.CreateScope();
IOptionsSnapshot<ExampleConfigBindingModel> config = 
    scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ExampleConfigBindingModel>>();
```

### Configuration Update Process

* make updates to config values in Azure App Configuration or Key Vault
* update the iteration tracking config value ie `config-version`
* wait the configured cache lifetime duration
* make a request to trigger the refresh - note this will not have access to the new value yet
* make another request, which will have the updated configuration values

## Notes

* Config only updates on requests after the cache has expired, but seems to update AFTER the triggering request complete, meaning it's not until the next request that the configuration is updated
* `IOptionsMonitor<T>` never updates - it seems that the app config updating mechanism differs from the standard config reloading system
* config loaded from azure doesn't need to have stubs in `appsettings.json`
