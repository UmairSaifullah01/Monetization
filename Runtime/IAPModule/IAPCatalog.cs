using System;
using System.Collections.Generic;


namespace THEBADDEST.MonetizationApi
{


	[Serializable]
	public class IAPCatalog
	{

		public List<IAPItem> items = new List<IAPItem>();
		public IAPItem Find(Predicate<IAPItem> item) => items.Find(item);

	}


}