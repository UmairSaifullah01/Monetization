using System;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Default catalog factory. Configuration registers <see cref="JsonKeyValueCatalog"/> at runtime.
	/// </summary>
	public static class CatalogFactory
	{
		public static Func<IKeyValueCatalog> Create { get; set; } = () => NullKeyValueCatalog.Instance;
	}
}
