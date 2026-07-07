namespace THEBADDEST.MonetizationApi
{
	public class JsonPlacementCatalog : IPlacementCatalog
	{
		public string Resolve(string category, string key)
		{
			JsonDataUtility.LoadData();
			return JsonDataUtility.GetData(category, key);
		}
	}
}
