using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{


    /// <summary>
    /// Abstract base class for all monetization modules.
    /// </summary>
    public abstract class MonetizationModule : ScriptableObject, IModule
    {
        [Header("Module Settings")]
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected string moduleName = "";

        protected bool isInitialized = false;
        protected bool isInitializing = false;

        /// <inheritdoc/>
        public bool IsEnabled => enabled;
        /// <inheritdoc/>
        public bool IsInitialized => isInitialized;
        /// <inheritdoc/>
        public bool IsInitializing => isInitializing;
        /// <inheritdoc/>
        public string ModuleName => string.IsNullOrEmpty(moduleName) ? GetType().Name : moduleName;

        /// <inheritdoc/>
        public virtual async UTask Initialize()
        {
            if (isInitialized)
            {
                SendLog.Log($"[{ModuleName}] Already initialized.");
                return;
            }

            if (isInitializing)
            {
                SendLog.LogWarning($"[{ModuleName}] Initialization already in progress.");
                await UTask.WaitUntil(() => !isInitializing);
                return;
            }

            if (!enabled)
            {
                SendLog.LogWarning($"[{ModuleName}] Module is disabled. Skipping initialization.");
                return;
            }

            isInitializing = true;

            try
            {
                await OnInitialize();
                isInitialized = true;
                SendLog.Log($"[{ModuleName}] Initialized successfully.");
            }
            catch (System.Exception ex)
            {
                SendLog.LogError($"[{ModuleName}] Initialization failed: {ex.Message}");
                throw;
            }
            finally
            {
                isInitializing = false;
            }
        }

        /// <summary>
        /// Override to implement module-specific initialization logic.
        /// </summary>
        protected virtual async UTask OnInitialize()
        {
            // Override in derived classes
            await UTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual void UpdateModule()
        {
            if (!enabled || !isInitialized)
            {
                return;
            }

            try
            {
                OnUpdateModule();
            }
            catch (System.Exception ex)
            {
                SendLog.LogError($"[{ModuleName}] Update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Override to implement module-specific update logic.
        /// </summary>
        protected virtual void OnUpdateModule()
        {
            // Override in derived classes
        }

        /// <inheritdoc/>
        public virtual void OnDestroy()
        {
            try
            {
                OnModuleDestroy();
            }
            catch (System.Exception ex)
            {
                SendLog.LogError($"[{ModuleName}] Destroy failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Override to implement module-specific cleanup logic.
        /// </summary>
        protected virtual void OnModuleDestroy()
        {
            // Override in derived classes for cleanup
        }

        /// <inheritdoc/>
        public virtual void Reset()
        {
            isInitialized = false;
            isInitializing = false;
        }

        /// <summary>
        /// Called by Unity when the script is loaded or a value changes in the inspector.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                moduleName = GetType().Name;
            }
        }
    }


}