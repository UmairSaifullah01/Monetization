using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.Database
{
	public interface IDatabaseModule : IModule
	{
		event Action<bool> OnSdkReady;

		void SetValue(string path, object value, Action<bool> onComplete = null);
		void GetValue(string path, Action<bool, object> onComplete);
		void UpdateChildren(string path, object value, Action<bool> onComplete = null);
		void DeleteValue(string path, Action<bool> onComplete = null);
		void ListenToValue(string path, Action<object> onValueChanged);
		void RemoveListener(string path);
	}
}
