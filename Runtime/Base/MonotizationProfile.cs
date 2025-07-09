using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monotization/MonotizationProfile", fileName = "MonotizationProfile", order = 0)]
public class MonotizationProfile : ScriptableObject,IEnumerable<MonotizationModule>
{
    // List of modules (MonotizationModule assets)
    public List<MonotizationModule> modules = new List<MonotizationModule>();

    public IEnumerator<MonotizationModule> GetEnumerator()
	{
		return modules.GetEnumerator();
	}

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

	public async void Initialize()
	{
		foreach (MonotizationModule module in modules)
		{
			await module.Initialize();
		}
	}

}