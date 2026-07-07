using System;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.Storage
{
	public interface IStorageModule : IModule
	{
		event Action<bool> OnSdkReady;

		void UploadBytes(string path, byte[] data, Action<bool> onComplete = null);
		void UploadFile(string path, string localFilePath, Action<bool> onComplete = null);
		void DownloadBytes(string path, Action<bool, byte[]> onComplete);
		void DownloadFile(string path, string localFilePath, Action<bool> onComplete = null);
		void GetDownloadUrl(string path, Action<bool, string> onComplete);
		void Delete(string path, Action<bool> onComplete = null);
		void GetMetadata(string path, Action<bool, StorageObjectMetadata> onComplete);
	}
}
