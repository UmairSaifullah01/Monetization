using System.Collections.Generic;

namespace THEBADDEST.MonetizationApi
{
	public static class IAPCatalogLoader
	{
		public const string IAPKeysCategory = "IAPKeys";

		public static List<IAPItem> LoadFromJson()
		{
			JsonDataUtility.Reload();
			var category = JsonDataUtility.GetCategory(IAPKeysCategory);
			var items = new List<IAPItem>();

			if (category == null || category.Count == 0)
			{
				return items;
			}

			foreach (var kvp in category)
			{
				if (string.IsNullOrEmpty(kvp.Value))
				{
					continue;
				}

				items.Add(new IAPItem
				{
					productId = kvp.Value,
					price = 0.99f,
					consumable = IsConsumable(kvp.Key, kvp.Value)
				});
			}

			return items;
		}

		public static void ApplyToCatalog(IAPCatalog catalog)
		{
			if (catalog == null)
			{
				return;
			}

			catalog.items = LoadFromJson();
		}

		private static bool IsConsumable(string key, string productId)
		{
			string combined = $"{key} {productId}".ToLowerInvariant();
			return combined.Contains("gem") ||
			       combined.Contains("coin") ||
			       combined.Contains("pack") ||
			       combined.Contains("consumable");
		}
	}
}
