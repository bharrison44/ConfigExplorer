# ConfigExplorer

POC to setup config explorer with AddAzureAppConfiguration

## Notes

* Azure services setup
    * Key Vault
        * TODO
    * App Configuration
        * TODO
* project setup
    * add package `Microsoft.Azure.AppConfiguration.AspNetCore`
    * rename default `appsettings.json` to `appsettings.example.json` and add the following

    ```json
    "ConnectionStrings": {
        "appConfig": ""
    }
    ```

    * add `appsettings.json` to the `.gitignore` file
    * copy the `appsettings.example.json` file to `appsettings.json` and populate the `appConfig` connection string with the value from the Azure App Configuration portal
* Azure App Configuration setup
    * add service registration to `Program.cs`/`Setup.cs`

    ```c#
    var builder = WebApplication.CreateBuilder(args);
    
    ...
    
    builder.Services.AddAzureAppConfiguration();
    ```

    * add config setup to `Program.cs`/`Setup.cs`
        * configuration prefix (ie: `example--`) optional - remove `Select` and `TrimKeyPrefix` if not needed
            * recommended if multiple apps using same app config instance
        * add `.UseFeatureFlag()` to `options` call chain if desired
        * use alternative Azure credentials, if needed
        * chain call `.SetCacheExpiration(Timespan)` after `.Register` to specify a different refresh interval - default is 30s

    ```c#
    var appConfigConnectionString = builder.Configuration.GetConnectionString("appConfig");
    builder
        .Configuration
        .AddAzureAppConfiguration(options =>
        {
            options.Connect(appConfigConnectionString)
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register("example--sentinel", refreshAll: true);
                })
                .Select("example--*")
                .TrimKeyPrefix("example--")
                .ConfigureKeyVault(config => config.SetCredential(new DefaultAzureCredential()));
        });
    ```

    * add service initialisation to `Program.cs`/`Setup.cs`

    ```c#
    var app = builder.Build();

    ...

    app.UseAzureAppConfiguration();
    ```

* usage in transient and scoped resources - ie controllers
    * use `IConfiguration` directly, or `IOptionsSnapshot<T>` if bound to a config model
    * do not use `IOption<T>` or `IOptionMonitor<T>` - these always contain initial values
* usage in singleton resources, background services, etc
    * can accept `IConfiguration` though the constructor, but not in any other form - none of them will update
    * `IOptionSnapshot<T>` can be used, but it is a scoped resource so cannot be gotten from the constructor (which would be wrong anyway). instead accept `IServiceProvider` from the constructor and do this when the configuration is needed

    ```c#
    using var scope = _ServiceProvider.CreateScope();
    IOptionsSnapshot<ExampleConfigBindingModel> config = 
        scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ExampleConfigBindingModel>>();
    ```

* config update process
    * make updates to config values in Azure App Configuration or Key Vault
    * update the sentinel
    * wait the configured cache lifetime duration
    * make a request to trigger the refresh
    * make another request, which will have the updated configuration values
* other things
    * Config only updates on requests after the cache has expired, but seems to update AFTER the triggering request complete, meaning it's not until the next request that the configuration is updated
    * `IOptionsMonitor<T>` never updates - it seems that the app config updating mechanism differs from the standard config reloading system
    * config loaded from azure doesn't need to have stubs in `appsettings.json`
