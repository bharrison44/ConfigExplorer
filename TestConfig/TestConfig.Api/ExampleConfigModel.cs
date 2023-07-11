namespace TestConfig.Api;

/// <summary>
/// A model for binding configuration.
/// </summary>
public class ExampleConfigModel
{
    /// <summary>
    /// A non-secret value.
    /// </summary>
    public string NonSecret { get; set; }

    /// <summary>
    /// A secret value referenced from KeyVault.
    /// </summary>
    public string KvSecret { get; set; }
}