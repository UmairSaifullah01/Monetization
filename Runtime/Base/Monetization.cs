using System;
using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{


	public static class Monetization
	{

		public static event Action<bool> OnInitialize;
		private static MonetizationProfile profile;

		public static T GetModule<T>() 
		{
			return profile.GetModule<T>();
		}

		public static async UTask Initialize()
		{
			if (profile != null)
			{
				return;
			}

			var profileObject = Resources.Load<MonetizationProfile>("MonetizationProfile");
			if (profileObject == null)
			{
				SendLog.LogError("MonetizationProfile object is missing in Resources folder.");
				return;
			}

			profile = profileObject;
			await profile.Initialize();
			OnInitialize?.Invoke(true);
			OnInitialize=null;
		}

		

	}


}