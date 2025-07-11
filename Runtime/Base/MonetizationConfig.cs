using System;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
    [CreateAssetMenu(menuName = "Monetization/Configuration", fileName = "MonetizationConfig", order = 1)]
    public class MonetizationConfig : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private LogLevel logLevel = LogLevel.Info;
        [SerializeField] private bool enablePerformanceLogging = false;
        [SerializeField] private float initializationTimeout = 30f;
        [SerializeField] private int maxRetryAttempts = 3;
        [SerializeField] private float retryDelaySeconds = 2f;

        [Header("Ads Settings")]
        [SerializeField] private bool enableAds = true;
        [SerializeField] private bool enableTestMode = true;
        [SerializeField] private float adLoadTimeout = 10f;
        [SerializeField] private int maxAdLoadAttempts = 3;

        [Header("IAP Settings")]
        [SerializeField] private bool enableIAP = true;
        [SerializeField] private bool enableReceiptValidation = true;
        [SerializeField] private float purchaseTimeout = 30f;

        [Header("Analytics Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableEventBatching = true;
        [SerializeField] private int batchSize = 10;
        [SerializeField] private float batchTimeout = 5f;

        [Header("Remote Config Settings")]
        [SerializeField] private bool enableRemoteConfig = true;
        [SerializeField] private float configFetchTimeout = 15f;
        [SerializeField] private bool enableConfigCaching = true;

        // Public properties
        public bool EnableDebugLogs => enableDebugLogs;
        public LogLevel LogLevel => logLevel;
        public bool EnablePerformanceLogging => enablePerformanceLogging;
        public float InitializationTimeout => initializationTimeout;
        public int MaxRetryAttempts => maxRetryAttempts;
        public float RetryDelaySeconds => retryDelaySeconds;

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

        private void OnEnable()
        {
            // Apply configuration
            SendLog.Enabled = enableDebugLogs;
            SendLog.CurrentLogLevel = logLevel;
        }

        public void ApplyConfiguration()
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