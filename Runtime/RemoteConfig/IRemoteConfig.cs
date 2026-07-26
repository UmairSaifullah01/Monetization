using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.RemoteConfig
{
	public interface IRemoteConfig<T> : IModule
	{
		event Action<bool> OnSdkReady;

		[Obsolete("Use OnSdkReady. Will be removed in a future version.")]
		event Action<bool> onInitialize;

		event Action OnDataLoad;

		void Load();

		void FetchConfig(Action<T> config);
	}
}
