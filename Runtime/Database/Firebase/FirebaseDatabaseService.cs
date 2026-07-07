using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using THEBADDEST.MonetizationApi;
using LogLevel = THEBADDEST.MonetizationApi.LogLevel;


namespace THEBADDEST.Database
{
	public class FirebaseDatabaseService
	{
		private readonly string _moduleName;
		private readonly Action<bool> _onSdkReady;
		private readonly Dictionary<string, EventHandler<ValueChangedEventArgs>> _listeners = new Dictionary<string, EventHandler<ValueChangedEventArgs>>();
		private DatabaseReference _rootReference;
		private bool _initComplete;

		public bool IsReady { get; private set; }

		public FirebaseDatabaseService(string moduleName, Action<bool> onSdkReady)
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
					_rootReference = FirebaseDatabase.DefaultInstance.RootReference;
					IsReady = _rootReference != null;
					_onSdkReady?.Invoke(IsReady);
					SendLog.LogModule(_moduleName, IsReady ? "Firebase Database initialized." : "Firebase Database root reference is null.", IsReady ? LogLevel.Info : LogLevel.Error);
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

		public void SetValue(string path, object value, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_rootReference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"SetValue failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void GetValue(string path, Action<bool, object> onComplete)
		{
			if (!EnsureReady(success => onComplete?.Invoke(false, null))) return;
			_rootReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					SendLog.LogModule(_moduleName, $"GetValue failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
					onComplete?.Invoke(false, null);
					return;
				}

				onComplete?.Invoke(true, task.Result?.Value);
			});
		}

		public void UpdateChildren(string path, object value, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_rootReference.Child(path).UpdateChildrenAsync(value as Dictionary<string, object>).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"UpdateChildren failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void DeleteValue(string path, Action<bool> onComplete)
		{
			if (!EnsureReady(onComplete)) return;
			_rootReference.Child(path).RemoveValueAsync().ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				if (!success)
				{
					SendLog.LogModule(_moduleName, $"DeleteValue failed for '{path}': {task.Exception?.Message}", LogLevel.Error);
				}

				onComplete?.Invoke(success);
			});
		}

		public void ListenToValue(string path, Action<object> onValueChanged)
		{
			if (!IsReady || _rootReference == null)
			{
				SendLog.LogModule(_moduleName, "Cannot listen: Firebase Database not initialized.", LogLevel.Warning);
				return;
			}

			RemoveListener(path);
			EventHandler<ValueChangedEventArgs> handler = (_, args) => onValueChanged?.Invoke(args.Snapshot?.Value);
			_listeners[path] = handler;
			_rootReference.Child(path).ValueChanged += handler;
		}

		public void RemoveListener(string path)
		{
			if (_rootReference == null || !_listeners.TryGetValue(path, out var handler))
			{
				return;
			}

			_rootReference.Child(path).ValueChanged -= handler;
			_listeners.Remove(path);
		}

		private bool EnsureReady(Action<bool> onComplete)
		{
			if (IsReady && _rootReference != null)
			{
				return true;
			}

			SendLog.LogModule(_moduleName, "Firebase Database is not ready.", LogLevel.Warning);
			onComplete?.Invoke(false);
			return false;
		}
	}
}
