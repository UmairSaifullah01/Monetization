using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
    [CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/Configuration", fileName = "MonetizationConfig", order = 1)]
    public class MonetizationConfig : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private LogLevel logLevel = LogLevel.Info;
        [SerializeField] private bool enablePerformanceLogging = false;
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelaySeconds = 2f;
        [Tooltip("If enabled, checks for internet connectivity before initializing modules.")]
        [SerializeField] private bool checkInternetBeforeInit = true;
        [Tooltip("If enabled, validates and removes duplicate modules on start.")]
        [SerializeField] private bool validateModulesOnStart = true;

        [Header("Ads Settings")]
        [Tooltip("Enable or disable all ad modules.")]
        [SerializeField] private bool enableAds = true;
        [Tooltip("Enable test mode for ads (use test ad units).")]
        [SerializeField] private bool enableTestMode = true;

        [Header("IAP Settings")]
        [Tooltip("Enable or disable all in-app purchase modules.")]
        [SerializeField] private bool enableIAP = true;

        [Header("Analytics Settings")]
        [Tooltip("Enable or disable all analytics modules.")]
        [SerializeField] private bool enableAnalytics = true;

        [Header("Remote Config Settings")]
        [Tooltip("Enable or disable remote config modules.")]
        [SerializeField] private bool enableRemoteConfig = true;
        [Tooltip("Timeout in seconds for fetching remote config.")]
        [SerializeField] private float configFetchTimeout = 15f;
        [Tooltip("Enable caching of remote config data.")]
        [SerializeField] private bool enableConfigCaching = true;

        public bool EnableDebugLogs => enableDebugLogs;
        public LogLevel LogLevel => logLevel;
        public bool EnablePerformanceLogging => enablePerformanceLogging;
        public int MaxRetryAttempts => maxRetryAttempts;
        public float RetryDelaySeconds => retryDelaySeconds;
        public bool CheckInternetBeforeInit => checkInternetBeforeInit;
        public bool ValidateModulesOnStart => validateModulesOnStart;

        public bool EnableAds => enableAds;
        public bool EnableTestMode => enableTestMode;

        public bool EnableIAP => enableIAP;

        public bool EnableAnalytics => enableAnalytics;

        public bool EnableRemoteConfig => enableRemoteConfig;
        public float ConfigFetchTimeout => configFetchTimeout;
        public bool EnableConfigCaching => enableConfigCaching;

        private static MonetizationConfig instance;
        public static MonetizationConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<MonetizationConfig>("MonetizationConfig");
                    if (instance == null)
                    {
                        SendLog.LogWarning("MonetizationConfig not found in Resources. Using default settings.");
                        instance = CreateInstance<MonetizationConfig>();
                    }
                }
                return instance;
            }
        }

        public void ApplySendLogConfiguration()
        {
            SendLog.Enabled = enableDebugLogs;
            SendLog.CurrentLogLevel = logLevel;
        }

        public void SetLogLevel(LogLevel newLevel)
        {
            logLevel = newLevel;
            SendLog.CurrentLogLevel = newLevel;
        }

        public void EnableModule(string moduleName, bool enabled)
        {
            switch (moduleName.ToLower())
            {
                case "ads":
                    enableAds = enabled;
                    break;
                case "iap":
                    enableIAP = enabled;
                    break;
                case "analytics":
                    enableAnalytics = enabled;
                    break;
                case "remoteconfig":
                    enableRemoteConfig = enabled;
                    break;
                default:
                    SendLog.LogWarning($"Unknown module: {moduleName}");
                    break;
            }
        }

        public bool IsModuleEnabled(string moduleName)
        {
            return moduleName.ToLower() switch
            {
                "ads" => enableAds,
                "iap" => enableIAP,
                "analytics" => enableAnalytics,
                "remoteconfig" => enableRemoteConfig,
                _ => false
            };
        }
    }
}
