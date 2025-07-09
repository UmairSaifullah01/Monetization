using System.Collections;
using UnityEngine;


namespace THEBADDEST.RemoteConfigSystem.Demo
{


	public class Test : MonoBehaviour
	{

		// [SerializeField] InterfaceReference<IRemoteConfig<object>> remoteConfig;

		public void Init()
		{
			// remoteConfig.Reference.OnInitialize += OnInitialize;
			// remoteConfig.Reference.Initialize();
		}

		void OnInitialize(bool init)
		{
			Debug.Log($"Initialized {init}");
		}

		public void Fetch()
		{
			// remoteConfig.Reference.FetchConfig(OnFetchConfig);
		}

		void OnFetchConfig(object config)
		{
			// Debug.Log($"FetchConfig {config}");
		}

	}


}