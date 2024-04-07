using Baracuda.Mediator.Callbacks;
using Baracuda.Tools;
using Baracuda.Utilities.Reflection;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;

namespace Baracuda.UI
{
    [AddressablesGroup("Mediator")]
    public partial class UIAsset : ScriptableAsset
    {
        #region Properties

        [ReadOnly]
        [field: NonSerialized]
        public bool IsLoaded { get; private set; }

        [ReadOnly]
        [field: NonSerialized]
        public bool IsLoading { get; private set; }

        [ReadOnly]
        [field: NonSerialized]
        public bool IsUnloading { get; private set; }

        #endregion


        #region Events

        /// <summary>
        ///     Called when the ui element was opened and has completed opening animations.
        /// </summary>
        public event Action Opened
        {
            add => _opened.Add(value);
            remove => _opened.Remove(value);
        }

        /// <summary>
        ///     Called when the ui element was closed and has completed closing animations.
        /// </summary>
        public event Action Closed
        {
            add => _closed.Add(value);
            remove => _closed.Remove(value);
        }

        /// <summary>
        ///     Called when the ui element is opened and has started opening animations.
        /// </summary>
        public event Action Opening
        {
            add => _opening.Add(value);
            remove => _opening.Remove(value);
        }

        /// <summary>
        ///     Called when the ui element is closed and has started closing animations.
        /// </summary>
        public event Action Closing
        {
            add => _closing.Add(value);
            remove => _closing.Remove(value);
        }

        #endregion


        #region Methods

        /// <summary>
        ///     Open the ui element with asynchronous opening animations.
        /// </summary>
        [Foldout("Runtime")]
        [Button]
        public void Open()
        {
            if (IsLoaded is false)
            {
                OpenAsync().Forget();
                return;
            }

            _uiInstance.Open();
        }

        /// <summary>
        ///     Open the ui element skipping asynchronous opening animations.
        /// </summary>
        [Button]
        public void OpenImmediate()
        {
            if (IsLoaded is false)
            {
                LoadImmediateInternal();
            }

            _uiInstance.OpenImmediate();
        }

        /// <summary>
        ///     Open the ui element with asynchronous opening animations.
        /// </summary>
        [Button]
        public async UniTask OpenAsync()
        {
            if (IsLoaded is false)
            {
                await LoadAsyncInternal();
            }
            await _uiInstance.OpenAsync();
        }

        /// <summary>
        ///     Close the ui element with asynchronous closing animations.
        /// </summary>
        [Line]
        [Button]
        public void Close()
        {
            CloseAsync().Forget();
        }

        /// <summary>
        ///     Close the ui element skipping asynchronous closing animations.
        /// </summary>
        [Button]
        public void CloseImmediate()
        {
            _uiInstance.CloseImmediate();
            if (unloadOnClose)
            {
                Unload();
            }
        }

        /// <summary>
        ///     Close the ui element with asynchronous closing animations.
        /// </summary>
        [Button]
        public async UniTask CloseAsync()
        {
            if (unloadOnClose)
            {
                await UnloadAsync();
            }
            else
            {
                await _uiInstance.CloseAsync();
            }
        }

        /// <summary>
        ///     Load the ui element, not opening it but loading the prefab into memory.
        /// </summary>
        [Line]
        [Button]
        public void Load()
        {
            LoadInternal();
        }

        /// <summary>
        ///     Load the ui element, not opening it but loading the prefab into memory.
        /// </summary>
        [Line]
        public UIComponent LoadImmediate()
        {
            LoadImmediateInternal();
            return _uiInstance;
        }

        /// <summary>
        ///     Load the ui element, not opening it but loading the prefab into memory and returning the instantiated component.
        /// </summary>
        [Line]
        public T LoadImmediate<T>() where T : UIComponent
        {
            LoadImmediateInternal();
            var instance = _uiInstance as T;
            if (instance == null)
            {
                Debug.LogError("View", $"Could not cast {_uiInstance} to {typeof(T)}");
            }
            return instance;
        }

        /// <summary>
        ///     Load the ui element, not opening it but loading the prefab into memory and returning the instantiated component.
        /// </summary>
        public async UniTask<UIComponent> LoadAsync()
        {
            await LoadAsyncInternal();
            return _uiInstance;
        }

        /// <summary>
        ///     Load the ui element, not opening it but loading the prefab into memory and returning the instantiated component.
        /// </summary>
        public async UniTask<T> LoadAsync<T>() where T : UIComponent
        {
            await LoadAsyncInternal();
            var instance = _uiInstance as T;
            if (instance == null)
            {
                Debug.LogError("View", $"Could not cast {_uiInstance} to {typeof(T)}");
            }
            return instance;
        }

        /// <summary>
        ///     Unload the ui element. This will close the ui and unload it from memory and returning the instantiated component.
        /// </summary>
        [Button]
        public void Unload()
        {
            CloseAndUnloadAsyncInternal().Forget();
        }

        /// <summary>
        ///     Unload the ui element. This will close the ui and unload it from memory.
        /// </summary>
        [Button]
        public async UniTask UnloadAsync()
        {
            await CloseAndUnloadAsyncInternal();
        }

        public bool ConsumeBackPressed()
        {
            if (IsLoaded)
            {
                return _uiInstance.TryConsumeReturn();
            }
            return false;
        }

        #endregion
    }
}