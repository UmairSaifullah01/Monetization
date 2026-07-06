using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.RemoteConfigSystem
{


	public abstract class RemoteConfigModule : MonetizationModule, IRemoteConfig<object>
	{

		public event Action<bool> onInitialize;
		public event Action OnDataLoad;

		public virtual IVariablesMapper variablesMapper { get; protected set; }


		public abstract void Load();

		public abstract void FetchConfig(Action<object> config);
		
		protected void RaiseSdkInitialized(bool success) => onInitialize?.Invoke(success);

		protected void OnDataLoadCompleted() => OnDataLoad?.Invoke();

	}


}
