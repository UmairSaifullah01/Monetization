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
        protected IModuleContext Context { get; private set; }

        /// <inheritdoc/>
        public bool IsEnabled => enabled;

        public void BindContext(IModuleContext context)
        {
            Context = context;
        }
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
                SendLog.LogModule(ModuleName, "Already initialized.");
                return;
            }

            if (isInitializing)
            {
                SendLog.LogModule(ModuleName, "Initialization already in progress.", LogLevel.Warning);
                await UTask.WaitUntil(() => !isInitializing);
                return;
            }

            if (!enabled)
            {
                SendLog.LogModule(ModuleName, "Module is disabled. Skipping initialization.", LogLevel.Warning);
                return;
            }

            isInitializing = true;

            try
            {
                await OnInitialize();
                isInitialized = true;
                SendLog.LogModule(ModuleName, "Initialized successfully.");
            }
            catch (System.Exception ex)
            {
                SendLog.LogModule(ModuleName, $"Initialization failed: {ex.Message}", LogLevel.Error);
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
            await UTask.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual void UpdateModule()
        {
            if (!enabled)
            {
                return;
            }

            try
            {
                OnUpdateModule();
            }
            catch (System.Exception ex)
            {
                SendLog.LogModule(ModuleName, $"Update failed: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Override to implement module-specific update logic.
        /// </summary>
        protected virtual void OnUpdateModule()
        {
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
                SendLog.LogModule(ModuleName, $"Destroy failed: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Override to implement module-specific cleanup logic.
        /// </summary>
        protected virtual void OnModuleDestroy()
        {
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
