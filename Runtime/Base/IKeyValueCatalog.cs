namespace THEBADDEST.MonetizationApi
{
	public interface IKeyValueCatalog
	{
		string Resolve(string category, string key);
	}
}
