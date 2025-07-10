using System;
using System.Collections;
using THEBADDEST.Advertisement;
using UnityEngine;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.RemoteConfigSystem.Demo
{


	public class Test : MonoBehaviour
	{

		void Awake()
		{
			Init();
		}

		public async void Init()
		{
			Monetization.OnInitialize += OnInitialize;
			await Monetization.Initialize();
		}

		void OnInitialize(bool init)
		{
			SendLog.Log($"Initialized {init}");
			Monetization.GetModule<IAdsModule>().LoadInterstitial();
		}

		public void ShowInterstitial()
		{
			Monetization.GetModule<IAdsModule>().ShowInterstitial();
		}
		public void Fetch()
		{
			// remoteConfig.Reference.FetchConfig(OnFetchConfig);
		}

		void OnFetchConfig(object config)
		{
			// Debug.Log($"FetchConfig {config}");
		}

	}


}