using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.RemoteConfigSystem
{
	public abstract class RemoteConfigModule : MonetizationModule, IRemoteConfig<object>
	{
		public event Action<bool> OnSdkReady;
		public event Action OnDataLoad;

		[Obsolete("Use OnSdkReady. Will be removed in a future version.")]
		public event Action<bool> onInitialize
		{
			add => OnSdkReady += value;
			remove => OnSdkReady -= value;
		}

		public virtual IVariablesMapper variablesMapper { get; protected set; }

		public abstract void Load();

		public abstract void FetchConfig(Action<object> config);

		protected void RaiseSdkInitialized(bool success) => OnSdkReady?.Invoke(success);

		protected void OnDataLoadCompleted() => OnDataLoad?.Invoke();
	}
}
