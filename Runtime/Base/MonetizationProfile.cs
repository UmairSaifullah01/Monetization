using System.Collections;
using System.Collections.Generic;
using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
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

		public async UTask Initialize()
		{
			SendLog.Enabled = debugLog;
			foreach (MonetizationModule module in modules)
			{
				await module.Initialize();
			}
		}

		internal T GetModule<T>() 
		{
			foreach (MonetizationModule module in modules)
			{
				if (module is T result)
				{
					return result;
				}
			}
			return default;
		}

		public void UpdateModules()
		{
			foreach (MonetizationModule module in modules)
			{
				module.UpdateModule();
			}
		}

	}


}