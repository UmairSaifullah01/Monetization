using System;
using THEBADDEST.MonetizationApi;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.RemoteConfig
{
	public abstract class RemoteConfigModule : MonetizationModule, IRemoteConfig<object>
	{
		[Header("Remote Config Settings")]
		[Tooltip("Timeout in seconds for fetching remote config.")]
		[SerializeField] protected float configFetchTimeout = 15f;
		[Tooltip("Enable caching of remote config data.")]
		[SerializeField] protected bool enableConfigCaching = true;

		public event Action<bool> OnSdkReady;
		public event Action OnDataLoad;

		public float ConfigFetchTimeout => configFetchTimeout;
		public bool EnableConfigCaching => enableConfigCaching;

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
