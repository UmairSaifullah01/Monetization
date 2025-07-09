using System;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.Monetization
{


	[Serializable]
	public class IAPItem
	{

		public string productId;
		public float  price;      
		public bool   NonConsumable=false;

	}
	[Serializable]
	public class IAPCatalog 
	{
		public List<IAPItem> items = new List<IAPItem>();

		public IAPItem Find(Predicate<IAPItem> item) => items.Find(item);
		
	}
	public interface IIAPModule
	{

		

	}

	public class IAPModule : MonetizationModule, IIAPModule
	{
		
	}


}

