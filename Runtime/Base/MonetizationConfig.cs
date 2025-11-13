using System;
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
        [SerializeField] private float initializationTimeout = 30f;
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
        [Tooltip("Timeout in seconds for ad loading.")]
        [SerializeField] private float adLoadTimeout = 10f;
        [Tooltip("Maximum number of attempts to load an ad before giving up.")]
        [SerializeField] private int maxAdLoadAttempts = 3;

        [Header("IAP Settings")]
        [Tooltip("Enable or disable all in-app purchase modules.")]
        [SerializeField] private bool enableIAP = true;
        [Tooltip("Enable receipt validation for purchases.")]
        [SerializeField] private bool enableReceiptValidation = true;
        [Tooltip("Timeout in seconds for purchase operations.")]
        [SerializeField] private float purchaseTimeout = 30f;

        [Header("Analytics Settings")]
        [Tooltip("Enable or disable all analytics modules.")]
        [SerializeField] private bool enableAnalytics = true;
        [Tooltip("Enable batching of analytics events.")]
        [SerializeField] private bool enableEventBatching = true;
        [Tooltip("Number of events to batch before sending.")]
        [SerializeField] private int batchSize = 10;
        [Tooltip("Timeout in seconds before sending a batch of events.")]
        [SerializeField] private float batchTimeout = 5f;

        [Header("Remote Config Settings")]
        [Tooltip("Enable or disable remote config modules.")]
        [SerializeField] private bool enableRemoteConfig = true;
        [Tooltip("Timeout in seconds for fetching remote config.")]
        [SerializeField] private float configFetchTimeout = 15f;
        [Tooltip("Enable caching of remote config data.")]
        [SerializeField] private bool enableConfigCaching = true;

        // Public properties for all settings
        public bool EnableDebugLogs => enableDebugLogs;
        public LogLevel LogLevel => logLevel;
        public bool EnablePerformanceLogging => enablePerformanceLogging;
        public float InitializationTimeout => initializationTimeout;
        public int MaxRetryAttempts => maxRetryAttempts;
        public float RetryDelaySeconds => retryDelaySeconds;
        public bool CheckInternetBeforeInit => checkInternetBeforeInit;
        public bool ValidateModulesOnStart => validateModulesOnStart;

        public bool EnableAds => enableAds;
        public bool EnableTestMode => enableTestMode;
        public float AdLoadTimeout => adLoadTimeout;
        public int MaxAdLoadAttempts => maxAdLoadAttempts;

        public bool EnableIAP => enableIAP;
        public bool EnableReceiptValidation => enableReceiptValidation;
        public float PurchaseTimeout => purchaseTimeout;

        public bool EnableAnalytics => enableAnalytics;
        public bool EnableEventBatching => enableEventBatching;
        public int BatchSize => batchSize;
        public float BatchTimeout => batchTimeout;

        public bool EnableRemoteConfig => enableRemoteConfig;
        public float ConfigFetchTimeout => configFetchTimeout;
        public bool EnableConfigCaching => enableConfigCaching;

        // Runtime configuration
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

        // Runtime configuration methods
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

        // Validation
        public bool ValidateConfiguration()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (initializationTimeout <= 0)
                errors.Add("Initialization timeout must be greater than 0");

            if (maxRetryAttempts < 0)
                errors.Add("Max retry attempts cannot be negative");

            if (adLoadTimeout <= 0)
                errors.Add("Ad load timeout must be greater than 0");

            if (purchaseTimeout <= 0)
                errors.Add("Purchase timeout must be greater than 0");

            if (batchSize <= 0)
                errors.Add("Batch size must be greater than 0");

            if (errors.Count > 0)
            {
                SendLog.LogError($"Configuration validation failed:\n{string.Join("\n", errors)}");
                return false;
            }

            return true;
        }
    }
}