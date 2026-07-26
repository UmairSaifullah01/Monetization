using System;
using THEBADDEST.MonetizationApi.Ads;
using UnityEngine;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Demo
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
			if (Monetization.TryGetModule<IAdsModule>(out var ads))
			{
				ads.LoadInterstitial();
			}
		}

		public void ShowInterstitial()
		{
			if (Monetization.TryGetModule<IAdsModule>(out var ads))
			{
				ads.ShowInterstitial();
			}
		}

		public void Fetch()
		{
		}

		void OnFetchConfig(object config)
		{
		}
	}
}
