using THEBADDEST.MonetizationApi.Ads;

namespace THEBADDEST.MonetizationApi
{
	public interface IModuleContext
	{
		IMonetizationSettings Settings { get; }
		IKeyValueCatalog Catalog { get; }
		IAdMetrics AdMetrics { get; }
	}

	public sealed class ModuleContext : IModuleContext
	{
		public IMonetizationSettings Settings { get; }
		public IKeyValueCatalog Catalog { get; }
		public IAdMetrics AdMetrics { get; }

		public ModuleContext(IMonetizationSettings settings, IKeyValueCatalog catalog, IAdMetrics adMetrics)
		{
			Settings = settings;
			Catalog = catalog ?? NullKeyValueCatalog.Instance;
			AdMetrics = adMetrics ?? NullAdMetrics.Instance;
		}
	}

	public sealed class NullKeyValueCatalog : IKeyValueCatalog
	{
		public static readonly NullKeyValueCatalog Instance = new NullKeyValueCatalog();
		public string Resolve(string category, string key) => null;
	}

	public sealed class NullAdMetrics : IAdMetrics
	{
		public static readonly NullAdMetrics Instance = new NullAdMetrics();
		public void RecordAdEvent(string adType, AdEventType eventType, string placement = null) { }
		public AdMetricSnapshot GetAdMetrics(string adType) => default;
		public System.Collections.Generic.IReadOnlyCollection<string> GetTrackedAdTypes() =>
			System.Array.Empty<string>();
		public void ResetAdMetrics(string adType = null) { }
	}
}
