namespace THEBADDEST.MonetizationApi
{
	public interface IPlacementCatalog
	{
		string Resolve(string category, string key);
	}
}
