using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.MonetizationApi.Storage
{
	public class FirebaseStorageModule : StorageModule
	{
		private FirebaseStorageService _service;

		protected override async UTask OnInitialize()
		{
			_service = new FirebaseStorageService(ModuleName, RaiseSdkInitialized);
			await _service.InitializeAsync();
		}

		public override void UploadBytes(string path, byte[] data, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.UploadBytes(path, data, onComplete);
		}

		public override void UploadFile(string path, string localFilePath, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.UploadFile(path, localFilePath, onComplete);
		}

		public override void DownloadBytes(string path, Action<bool, byte[]> onComplete)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false, null); return; }
			_service.DownloadBytes(path, onComplete);
		}

		public override void DownloadFile(string path, string localFilePath, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.DownloadFile(path, localFilePath, onComplete);
		}

		public override void GetDownloadUrl(string path, Action<bool, string> onComplete)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false, null); return; }
			_service.GetDownloadUrl(path, onComplete);
		}

		public override void Delete(string path, Action<bool> onComplete = null)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false); return; }
			_service.Delete(path, onComplete);
		}

		public override void GetMetadata(string path, Action<bool, StorageObjectMetadata> onComplete)
		{
			if (!EnsureReady()) { onComplete?.Invoke(false, null); return; }
			_service.GetMetadata(path, onComplete);
		}

		private bool EnsureReady()
		{
			if (_service == null || !_service.IsReady)
			{
				SendLog.LogModule(ModuleName, "Firebase Storage is not initialized.", LogLevel.Warning);
				return false;
			}

			return true;
		}
	}
}
