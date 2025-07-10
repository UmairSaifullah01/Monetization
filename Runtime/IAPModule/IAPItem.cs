using System;


namespace THEBADDEST.MonetizationApi
{


	[Serializable]
	public class IAPItem
	{

		public string productId;
		public float price;
		public bool consumable = false;
		public bool alreadyPurchased = false;
	}


}