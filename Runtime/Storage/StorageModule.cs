using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Storage
{
	public abstract class StorageModule : MonetizationModule, IStorageModule
	{
		public event Action<bool> OnSdkReady;

		protected void RaiseSdkInitialized(bool success) => OnSdkReady?.Invoke(success);

		public abstract void UploadBytes(string path, byte[] data, Action<bool> onComplete = null);
		public abstract void UploadFile(string path, string localFilePath, Action<bool> onComplete = null);
		public abstract void DownloadBytes(string path, Action<bool, byte[]> onComplete);
		public abstract void DownloadFile(string path, string localFilePath, Action<bool> onComplete = null);
		public abstract void GetDownloadUrl(string path, Action<bool, string> onComplete);
		public abstract void Delete(string path, Action<bool> onComplete = null);
		public abstract void GetMetadata(string path, Action<bool, StorageObjectMetadata> onComplete);
	}
}
