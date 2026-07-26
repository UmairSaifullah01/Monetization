using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Resolves keys from MonetizationKeys.json via JsonDataUtility.
	/// </summary>
	public class JsonKeyValueCatalog : IKeyValueCatalog
	{
		public string Resolve(string category, string key)
		{
			JsonDataUtility.LoadData();
			return JsonDataUtility.GetData(category, key);
		}
	}

	/// <summary>
	/// Backward-compatible alias for <see cref="JsonKeyValueCatalog"/>.
	/// </summary>
	public class JsonPlacementCatalog : JsonKeyValueCatalog
	{
	}

	internal static class CatalogFactoryBootstrap
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Register()
		{
			CatalogFactory.Create = () => new JsonKeyValueCatalog();
		}
	}
}
