# Monetization System

A comprehensive Unity monetization framework that provides a modular, extensible architecture for ads, in-app purchases, analytics, and remote configuration.

## Features

- **Modular Architecture**: Hot-swappable SDK providers via isolated asmdefs and installer groups
- **Resilient Initialization**: Failed modules are logged and skipped; remaining modules continue
- **Async/Await Support**: Modern async patterns for better performance
- **Performance Monitoring**: Built-in ad metrics (`LoadSuccessCount`, show counts, last show time)
- **Configuration Management**: Centralized configuration with runtime updates
- **ScriptableObject Configuration**: Runtime configuration without code changes

## Architecture Overview

```
Monetization (Static Entry Point)
├── MonetizationProfile (Configuration)
│   ├── IAdsModule provider (e.g. GoogleAdsModule)
│   ├── IIAPModule provider (e.g. UnityIAPModule)
│   ├── IAnalyticsModule provider (e.g. GAAnalyticsModule, FAAnalyticsModule)
│   ├── IRemoteConfig provider (e.g. FirebaseRemoteConfig)
│   ├── IDatabaseModule provider (e.g. FirebaseDatabaseModule)
│   └── IStorageModule provider (e.g. FirebaseStorageModule)
├── MonetizationConfig (Settings)
├── ModuleRegistry (interface-keyed cache)
└── PerformanceMonitor (Metrics)
```

### Assembly layout

| Assembly | Folder | SDK refs |
|----------|--------|----------|
| `THEBADDEST.Monetization.Core` | `Runtime/Base/` | None |
| `THEBADDEST.Monetization.Configuration` | `JsonDataUtility/` | None |
| `THEBADDEST.Monetization.Ads.Abstractions` | `Runtime/Ads/` | None |
| `THEBADDEST.Monetization.Ads.Google` | `Runtime/Ads/Google/` | AdMob |
| `THEBADDEST.Monetization.Ads.AppLovin` | `Runtime/Ads/AppLovin/` | AppLovin MAX |
| `THEBADDEST.Monetization.IAP.Unity` | `Runtime/IAPModule/Unity/` | Unity Purchasing |
| `THEBADDEST.Monetization.Analytics.GameAnalytics` | `Runtime/Analytics/GameAnalytics/` | GameAnalytics 7.10.6 |
| `THEBADDEST.Monetization.Analytics.Firebase` | `Runtime/Analytics/Firebase/` | Firebase Analytics |
| `THEBADDEST.Monetization.Analytics.Facebook` | `Runtime/Analytics/Facebook/` | Facebook Unity SDK |
| `THEBADDEST.Monetization.Analytics.Tenjin` | `Runtime/Analytics/Tenjin/` | Tenjin SDK |
| `THEBADDEST.Monetization.RemoteConfig.Firebase` | `Runtime/RemoteConfig/FireBaseRemoteConfig/` | Firebase Remote Config |
| `THEBADDEST.Monetization.Database.Abstractions` | `Runtime/Database/` | None |
| `THEBADDEST.Monetization.Database.Firebase` | `Runtime/Database/Firebase/` | Firebase Realtime Database |
| `THEBADDEST.Monetization.Storage.Abstractions` | `Runtime/Storage/` | None |
| `THEBADDEST.Monetization.Storage.Firebase` | `Runtime/Storage/Firebase/` | Firebase Cloud Storage |

Remove a provider assembly + UPM packages without breaking Core or Abstractions.

## Quick Start

### 1. Setup Configuration

Create a `MonetizationProfile` asset in your Resources folder and assign provider module assets (Ads, IAP, Analytics, Remote Config).

If you use **AppLovin MAX**, also set the SDK key in `Resources/MonetizationKeys.json`:

```json
{
  "AdKeys": {
    "MaxSdkKey": "your-applovin-max-sdk-key"
  }
}
```

### 2. Install (unified installer)

**Bootstrap (empty project):** Copy `Assets/Monetization/Installer/` + `Assets/Monetization/Logo/`, then open `Tools → Monetization → Installer` and click **Install Monetization**.

The installer:

1. Imports `MonetizationScripts.unitypackage`
2. Reads `Installer/installer_config.json` and installs **core** UPM packages + registries (UTask)
3. Opens the **Package Manager** panel in the same window after compilation

**Package Manager (same menu after install):** Install or uninstall SDK providers from `installer_config.json`. Local Firebase archives are copied from `Installer/Dependencies/*.tgz` per config.

Provider assemblies compile only when their UPM package is present (`versionDefines` + `defineConstraints`). See [`Installer/EXPORT.md`](Installer/EXPORT.md) for unitypackage export rules.

**Maintainers only:** Dev utilities (e.g. generate `installer_config.json` from manifest) live in [`Dev/`](Dev/) and are excluded from bootstrap and unitypackage exports. Menu: `Tools → Monetization Dev`.

### 3. Initialize the System

```csharp
using THEBADDEST.MonetizationApi;
using THEBADDEST.Advertisement;

public class GameManager : MonoBehaviour
{
    async void Start()
    {
        Monetization.OnInitialize += OnMonetizationInitialized;
        await Monetization.Initialize();
    }

    void OnMonetizationInitialized(bool success)
    {
        if (!success) return;

        if (Monetization.TryGetModule<IAdsModule>(out var ads))
        {
            ads.LoadInterstitial();
        }
    }
}
```

### 4. Use Modules Safely

Prefer `TryGetModule` so missing providers never throw:

```csharp
if (Monetization.TryGetModule<IAdsModule>(out var ads))
{
    ads.ShowInterstitial();
}

if (Monetization.TryGetModule<IIAPModule>(out var iap))
{
    iap.Purchase("product_id", OnSuccess, OnFail);
}

Analytics.SendEvent("level_complete", AnalyticsProviders.GameAnalytics | AnalyticsProviders.Firebase);

if (Monetization.TryGetModule<ITenjinAnalyticsModule>(out var tenjin))
{
    tenjin.SendAdImpression("applovin", 0.015d, "interstitial_home");
}

if (Monetization.TryGetModule<IRemoteConfig<object>>(out var remoteConfig))
{
    remoteConfig.FetchConfig(config => Debug.Log("Config loaded"));
}

if (Monetization.TryGetModule<IDatabaseModule>(out var database))
{
    database.SetValue("players/1/score", 100, success => Debug.Log($"Saved: {success}"));
}

if (Monetization.TryGetModule<IStorageModule>(out var storage))
{
    storage.UploadBytes("avatars/user.png", imageBytes, success => Debug.Log($"Uploaded: {success}"));
}
```

## SDK Events

Provider SDK readiness uses `OnSdkReady` (not lifecycle `OnInitialize()`):

```csharp
if (Monetization.TryGetModule<IAdsModule>(out var ads))
{
    ads.OnSdkReady += success => Debug.Log($"AdMob ready: {success}");
}
```

## Multi-Provider Analytics Routing

Use provider markers or the `Analytics` facade when different game flows should target different analytics SDKs.

```csharp
// Generic event to selected providers.
Analytics.SendEvent("level_complete", AnalyticsProviders.GameAnalytics | AnalyticsProviders.Firebase);

// Facebook-only purchase.
if (Monetization.TryGetModule<IFacebookAnalyticsModule>(out var facebook))
{
    facebook.LogPurchase(4.99f, "USD");
}

// Tenjin-only ad impression.
if (Monetization.TryGetModule<ITenjinAnalyticsModule>(out var tenjin))
{
    tenjin.SendAdImpression("applovin", 0.024d, "rewarded_level_end");
}
```

For orchestration scripts (ad events, IAP forwarding, campaign callbacks), use:

- `Monetization.GetModules<IAnalyticsModule>()` to iterate available analytics modules.
- Marker interfaces (`IGAAnalyticsModule`, `IFirebaseAnalyticsModule`, `IFacebookAnalyticsModule`, `ITenjinAnalyticsModule`) for provider-specific behavior.

Automatic ad-event routing was removed from base analytics modules so game code controls exactly where events are sent.

## Provider Notes

- `analytics_tenjin` installs via UPM Git URL: `https://github.com/tenjin/tenjin-unity-sdk.git#1.16.5`.
- `analytics_facebook` is wired in installer/profile, but Meta's official Unity SDK is typically imported manually (`FacebookSDK.unitypackage`) unless you vendor your own tgz.
- After these changes, re-export `Installer/MonetizationScripts.unitypackage` from Unity so bootstrap installers include the new modules.

## Swapping Providers

To replace AdMob with another network:

1. Add a new folder + asmdef implementing `IAdsModule` (reference `Ads.Abstractions` only).
2. Add an installer provider entry in `Installer/installer_config.json`.
3. Swap the module asset on `MonetizationProfile`.
4. Game code stays on `TryGetModule<IAdsModule>()` — no Core changes.

## Performance Monitoring

```csharp
var snapshot = PerformanceMonitor.Instance.GetAdMetrics(AdMetricsTypes.Interstitial);
Debug.Log($"Shows={snapshot.ShowCount}, LoadSuccess={snapshot.LoadSuccessCount}");

if (Monetization.TryGetModule<IAdsModule>(out var ads))
{
    int showCount = ads.GetInterstitialShowCount();
}
```

Ad loads respect `MonetizationConfig.AdLoadTimeout` via built-in timeout watchers.

## Resilient Initialization

If a module fails during init, remaining modules still initialize. Check failures via:

```csharp
await Monetization.Initialize();
foreach (string failure in Monetization.FailedModules)
{
    Debug.LogWarning(failure);
}
```

`Monetization.OnError` fires with a summary when any module fails (non-throwing).

## Hot-Swap Checklist

1. Open `Tools → Monetization → Validate Hot-Swap Readiness` after changing providers.
2. Remove provider folder/asmdef + uninstall UPM packages via Installer.
3. Remove or swap the module asset on `MonetizationProfile`.
4. Confirm game code uses `TryGetModule<T>()` only — never concrete provider types.
5. Core + Abstractions must compile with zero SDK references.

## GameAnalytics UPM Note

Install GameAnalytics via the Monetization Installer (`com.gameanalytics.sdk` from OpenUPM). The legacy `.unitypackage` GA SDK has no asmdef and cannot be referenced from provider assemblies.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `TryGetModule<IAdsModule>` returns false | Install `ads_google` provider; assign `GoogleAdsModule` on profile |
| IAP catalog empty | Click **Sync IAP Catalog** on profile inspector or populate `IAPKeys` in `MonetizationKeys.json` |
| Firebase RC mapper missing | Assign `Content/FireBaseVariableMapper.asset` on `FirebaseRemoteConfig` module |
| Profile warns about missing asmdef | Re-install provider via Installer or remove module from profile |
| GA compile errors after install | Use UPM `com.gameanalytics.sdk`, not legacy unitypackage |

## Project Settings Sync

Project settings (package name, version, keystore) sync from `MonetizationKeys.json` via:

- **Editor**: Sync Project button on Monetization Profile inspector
- **Build**: `BuildProcessor` calls `ProjectSettingsSync.SyncFromJson()` automatically

Store keystore passwords locally in `MonetizationKeys.json` under `ProjectKeys` (template ships with empty passwords).

## Extending the System

### Adding a New Ad Network

```csharp
public class AppLovinAdsModule : AdsModule
{
    protected override async UTask OnInitialize()
    {
        // Initialize SDK, then RaiseAdsSdkReady(true);
        await UTask.CompletedTask;
    }

    public override IAppAd FetchInterstitial(string placement = "default") { /* ... */ }
    // Implement other IAdsModule methods
}
```

## Version History

- **v4.0b Phase 2**: Asmdef split, hot-swappable providers, resilient init, `TryGetModule`, `OnSdkReady`, runtime services
- **v4.0b Phase 1**: Unified lifecycle, ad metrics, editor-safe module lookup

## License

MIT License — see LICENSE for details.
