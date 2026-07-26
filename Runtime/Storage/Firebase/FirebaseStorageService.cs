using System;
using System.IO;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Storage
{
	public class FirebaseStorageService
	{
		private readonly string _moduleName;
		private readonly Action<bool> _onSdkReady;
		private FirebaseStorage _storage;
		private bool _initComplete;

		public bool IsReady { get; private set; }

		public FirebaseStorageService(string moduleName, Action<bool> onSdkReady)
		{
			_moduleName = moduleName;
			_onSdkReady = onSdkReady;
		}

		public async THEBADDEST.Tasks.UTask InitializeAsync()
		{
			_initComplete = false;
			FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					IsReady = false;
					_onSdkReady?.Invoke(false);
					_initComplete = true;
					return;
				}

				if (task.Result == DependencyStatus.Available)
				{
					_storage = FirebaseStorage.DefaultInstance;
					IsReady = _storage != null;
					_onSdkReady?.Invoke(IsReady);
					SendLog.LogModule(_moduleName, IsReady ? "Firebase Storage initialized." : "Firebase Storage instance is null.", IsReady ? LogLevel.Info : LogLevel.Error);
				}
				else
				{
					IsReady = false;
					_onSdkReady?.Invoke(false);
					SendLog.LogModule(_moduleName, $"Firebase dependencies unavailable: {task.Result}", LogLevel.Error);
				}

				_initComplete = true;
			});

			await THEBADDEST.Tasks.UTask.WaitUntil(() => _initComplete);
		}

		public void UploadBytes(string path, byte[] data, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_storage.GetReference(path).PutBytesAsync(data).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"UploadBytes failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void UploadFile(string path, string localFilePath, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			if (!File.Exists(localFilePath))
			{
				SendLog.LogModule(_moduleName, $"UploadFile failed: local file not found '{localFilePath}'.", LogLevel.Error);
				onComplete?.Invoke(false);
				return;
			}

			_storage.GetReference(path).PutFileAsync(localFilePath).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"UploadFile failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void DownloadBytes(string path, Action<bool, byte[]> onComplete)
		{
			if (!EnsureReady(success => onComplete?.Invoke(false, null))) return;
			_storage.GetReference(path).GetBytesAsync(long.MaxValue).ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					SendLog.LogModule(_moduleName, $"DownloadBytes failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
					onComplete?.Invoke(false, null);
					return;
				}

				onComplete?.Invoke(true, task.Result);
			});
		}

		public void DownloadFile(string path, string localFilePath, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_storage.GetReference(path).WriteToFileAsync(localFilePath).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"DownloadFile failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void GetDownloadUrl(string path, Action<bool, string> onComplete)
		{
			if (!EnsureReady(success => onComplete?.Invoke(false, null))) return;
			_storage.GetReference(path).GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					SendLog.LogModule(_moduleName, $"GetDownloadUrl failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
					onComplete?.Invoke(false, null);
					return;
				}

				onComplete?.Invoke(true, task.Result?.ToString());
			});
		}

		public void Delete(string path, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_storage.GetReference(path).DeleteAsync().ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"Delete failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void GetMetadata(string path, Action<bool, StorageObjectMetadata> onComplete)
		{
			if (!EnsureReady(success => onComplete?.Invoke(false, null))) return;
			_storage.GetReference(path).GetMetadataAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					SendLog.LogModule(_moduleName, $"GetMetadata failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
					onComplete?.Invoke(false, null);
					return;
				}

				var metadata = task.Result;
				onComplete?.Invoke(true, new StorageObjectMetadata
				{
					Name = metadata?.Name,
					SizeBytes = metadata?.SizeBytes ?? 0,
					ContentType = metadata?.ContentType
				});
			});
		}

		private bool EnsureReady(Action<bool> onComplete)
		{
			if (IsReady && _storage != null)
			{
				return true;
			}

			SendLog.LogModule(_moduleName, "Firebase Storage is not ready.", LogLevel.Warning);
			onComplete?.Invoke(false);
			return false;
		}
	}
}
