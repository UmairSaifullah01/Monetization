using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{


    public abstract class MonetizationModule : ScriptableObject
    {
        [Header("Module Settings")]
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected string moduleName = "";

        protected bool isInitialized = false;
        protected bool isInitializing = false;

        public bool IsEnabled => enabled;
        public bool IsInitialized => isInitialized;
        public bool IsInitializing => isInitializing;
        public string ModuleName => string.IsNullOrEmpty(moduleName) ? GetType().Name : moduleName;

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

        protected virtual async UTask OnInitialize()
        {
            // Override in derived classes
            await UTask.CompletedTask;
        }

        internal virtual void UpdateModule()
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

        protected virtual void OnUpdateModule()
        {
            // Override in derived classes
        }

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

        protected virtual void OnModuleDestroy()
        {
            // Override in derived classes for cleanup
        }

        public virtual void Reset()
        {
            isInitialized = false;
            isInitializing = false;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                moduleName = GetType().Name;
            }
        }
    }


}