using System;
using System.Collections.Generic;
using System.Linq;

namespace THEBADDEST.MonetizationApi
{
	public interface IModuleRegistry
	{
		void Register(IModule module);
		void Clear();
		T Get<T>() where T : class, IModule;
		bool TryGet<T>(out T module) where T : class, IModule;
		IReadOnlyList<T> GetAll<T>() where T : class, IModule;
		IReadOnlyList<IModule> All { get; }
	}

	public class ModuleRegistry : IModuleRegistry
	{
		private readonly Dictionary<Type, IModule> _modules = new Dictionary<Type, IModule>();
		private readonly List<IModule> _all = new List<IModule>();

		public IReadOnlyList<IModule> All => _all;

		public void Clear()
		{
			_modules.Clear();
			_all.Clear();
		}

		public void Register(IModule module)
		{
			if (module == null || !module.IsInitialized)
			{
				return;
			}

			var concreteType = module.GetType();
			_modules[concreteType] = module;

			foreach (var iface in concreteType.GetInterfaces().Where(t => typeof(IModule).IsAssignableFrom(t) && t != typeof(IModule)))
			{
				_modules[iface] = module;
			}

			if (!_all.Contains(module))
			{
				_all.Add(module);
			}
		}

		public T Get<T>() where T : class, IModule
		{
			if (_modules.TryGetValue(typeof(T), out var module))
			{
				return module as T;
			}

			return default;
		}

		public bool TryGet<T>(out T module) where T : class, IModule
		{
			module = Get<T>();
			return module != null;
		}

		public IReadOnlyList<T> GetAll<T>() where T : class, IModule
		{
			var matches = new List<T>();
			for (int i = 0; i < _all.Count; i++)
			{
				if (_all[i] is T typedModule)
				{
					matches.Add(typedModule);
				}
			}

			return matches;
		}
	}
}
