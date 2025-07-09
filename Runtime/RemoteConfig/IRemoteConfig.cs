using System;


namespace THEBADDEST.RemoteConfigSystem
{


	public interface IRemoteConfig<T> 
	{

		event Action<bool> OnInitialize;
		event Action OnDataLoad;
		
		void Load();

		void FetchConfig(Action<T> config);
		
		

	}

}