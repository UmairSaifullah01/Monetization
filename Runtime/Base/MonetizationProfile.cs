using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.Monetization
{


	[CreateAssetMenu(menuName = "Monotization/MonotizationProfile", fileName = "MonotizationProfile", order = 0)]
	public class MonetizationProfile : ScriptableObject, IEnumerable<MonetizationModule>
	{

		[SerializeField] bool debugLog = true;
		// List of modules (MonotizationModule assets)
		public List<MonetizationModule> modules = new List<MonetizationModule>();

		public IEnumerator<MonetizationModule> GetEnumerator()
		{
			return modules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public async void Initialize()
		{
			foreach (MonetizationModule module in modules)
			{
				await module.Initialize();
			}
		}

	}


}