using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Database
{
	public class FirebaseDatabaseModule : DatabaseModule
	{
		private FirebaseDatabaseService _service;

		protected override async UTask OnInitialize()
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableDatabase)
			{
				SendLog.LogModule(ModuleName, "Database is disabled by MonetizationConfig.", LogLevel.Warning);
				return;
			}

			_service = new FirebaseDatabaseService(ModuleName, RaiseSdkInitialized);
			await _service.InitializeAsync();
		}

		protected override void OnModuleDestroy()
		{
			base.OnModuleDestroy();
		}

		public override void SetValue(string path, object value, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.SetValue(path, value, onComplete);
		}

		public override void GetValue(string path, Action<bool, object> onComplete)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false, null); return; }
			_service.GetValue(path, onComplete);
		}

		public override void UpdateChildren(string path, object value, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.UpdateChildren(path, value, onComplete);
		}

		public override void DeleteValue(string path, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.DeleteValue(path, onComplete);
		}

		public override void ListenToValue(string path, Action<object> onValueChanged)
		{
			if (!EnsureReady()) return;
			_service.ListenToValue(path, onValueChanged);
		}

		public override void RemoveListener(string path)
		{
			_service?.RemoveListener(path);
		}

		private bool EnsureReady()
		{
			if (!MonetizationConfig.Instance.EnableDatabase)
			{
				SendLog.LogModule(ModuleName, "Database is disabled by MonetizationConfig.", LogLevel.Warning);
				return false;
			}

			if (_service == null || !_service.IsReady)
			{
				SendLog.LogModule(ModuleName, "Firebase Database is not initialized.", LogLevel.Warning);
				return false;
			}

			return true;
		}
	}
}
