using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.RemoteConfigSystem
{


	public interface IRemoteConfig<T>  : IModule
	{

		event Action<bool> OnInitialize;
		event Action OnDataLoad;
		
		void Load();

		void FetchConfig(Action<T> config);
	}

}