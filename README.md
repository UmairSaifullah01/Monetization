# Monetization System

A comprehensive Unity monetization framework that provides a modular, extensible architecture for ads, in-app purchases, analytics, and remote configuration.

## Features

- **Modular Architecture**: Easy to add new ad networks, analytics providers, or IAP platforms
- **Async/Await Support**: Modern async patterns for better performance
- **Error Handling**: Comprehensive error handling with retry mechanisms
- **Performance Monitoring**: Built-in performance tracking and reporting
- **Configuration Management**: Centralized configuration with runtime updates
- **Enhanced Logging**: Multi-level logging system with filtering
- **ScriptableObject Configuration**: Runtime configuration without code changes

## Architecture Overview

```
Monetization (Static Entry Point)
├── MonetizationProfile (Configuration)
│   ├── AdsModule (Google Ads, etc.)
│   ├── IAPModule (Unity IAP, etc.)
│   ├── AnalyticsModule (Game Analytics, etc.)
│   └── RemoteConfigModule (Firebase, etc.)
├── MonetizationConfig (Settings)
└── PerformanceMonitor (Metrics)
```

## Quick Start

### 1. Setup Configuration

Create a `MonetizationProfile` asset in your Resources folder:

```csharp
// Create via menu: Assets > Create > Monetization > MonetizationProfile
// Add your modules (Ads, IAP, Analytics, RemoteConfig)
```

### 2. Install Dependencies (Recommended)

Open the Monetization Installer from the Unity Editor:

- Go to `Tools → Monetization → Installer`
- Click the **Install** button. The installer will automatically fetch and install all required dependencies for you.

No manual manifest editing required!

### 3. Initialize the System

```csharp
using THEBADDEST.MonetizationApi;
using THEBADDEST.Advertisement;

public class GameManager : MonoBehaviour
{
    async void Start()
    {
        // Subscribe to initialization events
        Monetization.OnInitialize += OnMonetizationInitialized;
        Monetization.OnError += OnMonetizationError;

        // Initialize the system
        await Monetization.Initialize();
    }

    void OnMonetizationInitialized(bool success)
    {
        if (success)
        {
            Debug.Log("Monetization system ready!");
            ShowAds();
        }
    }

    void OnMonetizationError(string error)
    {
        Debug.LogError($"Monetization error: {error}");
    }
}
```

### 3. Use Modules

```csharp
// Get modules
var adsModule = Monetization.GetModule<IAdsModule>();
var iapModule = Monetization.GetModule<IIAPModule>();
var analyticsModule = Monetization.GetModule<IAnalyticsModule>();
var remoteConfig = Monetization.GetModule<IRemoteConfig<object>>();

// Show ads
adsModule.ShowInterstitial();
adsModule.ShowRewarded("reward_placement",
    reward => Debug.Log("Reward claimed!"),
    () => Debug.Log("Ad failed"));

// Make purchases
iapModule.Purchase("product_id",
    () => Debug.Log("Purchase successful!"),
    () => Debug.Log("Purchase failed"));

// Send analytics
analyticsModule.SendEvent("level_complete");
analyticsModule.SendDesignEvent("Gameplay", "Level", "Complete", 1.0f);

// Fetch remote config
remoteConfig.FetchConfig(config => Debug.Log("Config loaded"));
```

## Module Types

### Ads Module

- **Google Ads**: Full Google Mobile Ads integration with consent management
- **Extensible**: Easy to add other ad networks (Facebook, Unity Ads, etc.)

### IAP Module

- **Unity IAP**: Complete Unity In-App Purchasing integration
- **Receipt Validation**: Built-in receipt validation
- **Product Management**: Easy product catalog management

### Analytics Module

- **Game Analytics**: Full Game Analytics integration
- **Event Batching**: Automatic event batching for better performance
- **Custom Events**: Support for custom event types

### Remote Config Module

- **Firebase Remote Config**: Complete Firebase integration
- **Variable Mapping**: Easy remote variable management
- **Caching**: Built-in caching for offline support

## Configuration

### MonetizationConfig

Centralized configuration for all modules:

```csharp
var config = MonetizationConfig.Instance;

// Runtime configuration
config.SetLogLevel(LogLevel.Debug);
config.EnableModule("ads", true);
config.EnableModule("iap", false);

// Validation
if (config.ValidateConfiguration())
{
    Debug.Log("Configuration is valid");
}
```

### Performance Monitoring

```csharp
var monitor = PerformanceMonitor.Instance;

// Monitor operations
monitor.StartOperation("ad_load");
// ... perform operation
monitor.EndOperation("ad_load");

// Get statistics
float avgDuration = monitor.GetAverageDuration("ad_load");
float successRate = monitor.GetSuccessRate("ad_load");

// Generate report
monitor.LogPerformanceReport();
```

## Error Handling

The system includes comprehensive error handling:

```csharp
try
{
    await Monetization.Initialize();
}
catch (TimeoutException ex)
{
    Debug.LogError("Initialization timed out");
}
catch (Exception ex)
{
    Debug.LogError($"Initialization failed: {ex.Message}");
}
```

## Best Practices

### 1. Initialization

- Always check `Monetization.IsInitialized` before using modules
- Subscribe to `OnInitialize` and `OnError` events
- Use try-catch blocks for error handling

### 2. Module Usage

- Cache module references for better performance
- Check module availability before use
- Handle null returns from `GetModule<T>()`

### 3. Configuration

- Validate configuration before initialization
- Use appropriate log levels for production
- Enable only necessary modules

### 4. Performance

- Monitor critical operations
- Use async/await patterns
- Implement proper error handling

## Extending the System

### Adding a New Ad Network

```csharp
[CreateAssetMenu(menuName = "Monetization/AdsModule/FacebookAds", fileName = "FacebookAdsModule", order = 11)]
public class FacebookAdsModule : AdsModule
{
    public override void Init()
    {
        // Initialize Facebook Ads SDK
    }

    public override IAppAd FetchBanner(string placement = "default")
    {
        return new FacebookBannerAd(placement);
    }

    // Implement other ad types...
}
```

### Adding a New Analytics Provider

```csharp
[CreateAssetMenu(menuName = "Monetization/AnalyticsModule/FirebaseAnalytics", fileName = "FirebaseAnalyticsModule", order = 12)]
public class FirebaseAnalyticsModule : AnalyticsModule
{
    public override void SendEvent(string name)
    {
        // Send to Firebase Analytics
    }

    // Implement other methods...
}
```

## Troubleshooting

### Common Issues

1. **Module not found**: Ensure the module is added to the MonetizationProfile
2. **Initialization fails**: Check configuration and network connectivity
3. **Ads not showing**: Verify ad unit IDs and consent status
4. **IAP not working**: Check product configuration and store setup

### Debug Mode

Enable debug logging for troubleshooting:

```csharp
SendLog.CurrentLogLevel = LogLevel.Debug;
SendLog.Enabled = true;
```

## Version History

- **v4.0b**: Enhanced error handling, performance monitoring, configuration management
- **v3.0**: Added async/await support and improved module architecture
- **v2.0**: Modular design with interface-based architecture
- **v1.0**: Initial release with basic monetization features

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please refer to the documentation or create an issue in the repository.
