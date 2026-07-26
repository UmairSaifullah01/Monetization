namespace THEBADDEST.MonetizationApi
{
	public interface IMonetizationSettings
	{
		bool EnableDebugLogs { get; }
		LogLevel LogLevel { get; }
		bool EnablePerformanceLogging { get; }
		int MaxRetryAttempts { get; }
		float RetryDelaySeconds { get; }
		bool CheckInternetBeforeInit { get; }
		bool ValidateModulesOnStart { get; }
		bool UseKeyStore { get; }
	}
}
