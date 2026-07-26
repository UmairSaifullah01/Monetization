using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Database
{
	public abstract class DatabaseModule : MonetizationModule, IDatabaseModule
	{
		public event Action<bool> OnSdkReady;

		protected void RaiseSdkInitialized(bool success) => OnSdkReady?.Invoke(success);

		public abstract void SetValue(string path, object value, Action<bool> onComplete = null);
		public abstract void GetValue(string path, Action<bool, object> onComplete);
		public abstract void UpdateChildren(string path, object value, Action<bool> onComplete = null);
		public abstract void DeleteValue(string path, Action<bool> onComplete = null);
		public abstract void ListenToValue(string path, Action<object> onValueChanged);
		public abstract void RemoveListener(string path);
	}
}
