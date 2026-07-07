using THEBADDEST.Tasks;
using UnityEngine;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.RemoteConfigSystem
{
	public class FirebaseRemoteConfig : RemoteConfigModule
	{
		[SerializeField] RemoteVariablesMapper m_VariablesMapper;
		public override IVariablesMapper variablesMapper => m_VariablesMapper;

		private FirebaseRemoteConfigService _service;

		protected override async UTask OnInitialize()
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableRemoteConfig)
			{
				SendLog.LogModule(ModuleName, "Remote Config is disabled by MonetizationConfig.", LogLevel.Warning);
				return;
			}

			_service = new FirebaseRemoteConfigService(m_VariablesMapper, ModuleName, RaiseSdkInitialized, OnDataLoadCompleted);
			await _service.InitializeAsync();
		}

		public override void Load() => _service?.Load();

		public override void FetchConfig(System.Action<object> config) => _service?.FetchConfig(config);
	}
}
