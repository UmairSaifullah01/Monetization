using System;
using THEBADDEST.Tasks;


namespace THEBADDEST.RemoteConfigSystem
{


	public abstract class RemoteConfigModule : MonotizationModule, IRemoteConfig<object>
	{

		public event Action<bool> OnInitialize;
		public event Action OnDataLoad;

		public virtual IVariablesMapper variablesMapper { get; protected set; }
		public bool IsInitialized { get; protected set; }


		public override async UTask Initialize()
		{
			IsInitialized = false;
			OnInitialize += OnInitializeCompleted;

			void OnInitializeCompleted(bool success)
			{
				IsInitialized = success;
				OnInitialize -= OnInitializeCompleted;
			}
		}


		public abstract void Load();

		public abstract void FetchConfig(Action<object> config);
		
		protected void OnInitializeCompleted(bool success) => OnInitialize?.Invoke(success);

		protected void OnDataLoadCompleted() => OnDataLoad?.Invoke();

	}


}