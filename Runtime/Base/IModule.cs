using THEBADDEST.Tasks;


namespace THEBADDEST.MonetizationApi
{


	/// <summary>
	/// Base interface for all monetization modules.
	/// </summary>
	public interface IModule
	{
		/// <summary>
		/// Gets whether the module is initialized.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Gets whether the module is currently initializing.
		/// </summary>
		bool IsInitializing { get; }

		/// <summary>
		/// Gets the module's name.
		/// </summary>
		string ModuleName { get; }

		/// <summary>
		/// Initializes the module asynchronously.
		/// </summary>
		UTask Initialize();

		/// <summary>
		/// Resets the module to its uninitialized state.
		/// </summary>
		void Reset();

		/// <summary>
		/// Called when the module is destroyed.
		/// </summary>
		void OnDestroy();

		/// <summary>
		/// Updates the module (called every frame or tick).
		/// </summary>
		void UpdateModule();
	}


}