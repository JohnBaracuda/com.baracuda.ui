using Baracuda.Mediator.Callbacks;
using Baracuda.Mediator.Events;
using Baracuda.Tools;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Baracuda.UI
{
    public partial class UIAsset
    {
        #region Fields

        [Foldout("View")]
        [Tooltip("Reference to the ViewComponent prefab")]
        [SerializeField] private AssetReferenceT<GameObject> viewPrefabReference;
        [Tooltip("When enabled, the prefab is loaded at the beginning of the game")]
        [SerializeField] private bool preload;
        [Tooltip("When enabled, the prefab is unloaded when closed")]
        [SerializeField] private bool unloadOnClose;

        [ReadOnly]
        [NonSerialized] private UIComponent _uiInstance;
        [NonSerialized] private AsyncOperationHandle<GameObject> _handle;

        private readonly Broadcast _opened = new();
        private readonly Broadcast _closed = new();
        private readonly Broadcast _opening = new();
        private readonly Broadcast _closing = new();

        #endregion


        #region Setup & Shutdown

        [CallbackOnInitialization]
        private void Initialize()
        {
            if (preload)
            {
                Load();
            }
        }

        [CallbackOnEnterEditMode]
        private void OnExitPlayMode()
        {
            IsUnloading = false;
            IsLoading = false;
            IsLoaded = false;
            _closing.Clear();
            _opening.Clear();
            _closed.Clear();
            _opened.Clear();
            _handle = default(AsyncOperationHandle<GameObject>);

            if (_uiInstance != null)
            {
                _uiInstance.StateChanged -= OnStateChanged;
                viewPrefabReference.ReleaseInstance(_uiInstance.gameObject);
                Destroy(_uiInstance.gameObject);
                _uiInstance = null;
            }
        }

        #endregion


        #region Unload

        private async UniTask CloseAndUnloadAsyncInternal()
        {
            if (IsLoaded is false)
            {
                return;
            }

            await _uiInstance.CloseAsync();

            UnloadInternal();
        }

        private void UnloadInternal()
        {
            if (IsLoaded is false)
            {
                Assert.IsNull(_uiInstance);
                return;
            }

            Assert.IsNotNull(_uiInstance);
            Tween.StopAll(this);
            _uiInstance.StateChanged -= OnStateChanged;
            viewPrefabReference.ReleaseInstance(_uiInstance.gameObject);
            viewPrefabReference.ReleaseAsset();
            Destroy(_uiInstance.gameObject);
            _uiInstance = null;
            _handle = default(AsyncOperationHandle<GameObject>);
            IsLoaded = false;
        }

        #endregion


        #region Loading

        private void LoadInternal()
        {
            LoadAsyncInternal().Forget();
        }

        private async UniTask LoadAsyncInternal()
        {
            if (IsLoaded)
            {
                return;
            }
            if (IsLoading)
            {
                await UniTask.WaitUntil(() => IsLoaded);
                return;
            }

            IsLoading = true;
            Assert.IsNull(_uiInstance);
            IsLoaded = false;
            _handle = viewPrefabReference.LoadAssetAsync();
            await _handle;
            var gameObject = Instantiate(_handle.Result);
            gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);
            _uiInstance = gameObject.GetComponent<UIComponent>();
            _uiInstance.Asset = this;
            _uiInstance.StateChanged += OnStateChanged;
            IsLoading = false;
            IsLoaded = true;
        }

        private void LoadImmediateInternal()
        {
            if (_handle.IsValid() && _handle.PercentComplete > 0)
            {
                return;
            }

            IsLoading = true;
            Assert.IsNull(_uiInstance);
            IsLoaded = false;
            _handle = viewPrefabReference.InstantiateAsync();
            var gameObject = _handle.WaitForCompletion();
            gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);
            _uiInstance = gameObject.GetComponent<UIComponent>();
            _uiInstance.Asset = this;
            _uiInstance.StateChanged += OnStateChanged;
            IsLoading = false;
            IsLoaded = true;
        }

        #endregion


        #region Instance Callbacks

        private void OnStateChanged(ViewState state)
        {
            switch (state)
            {
                case ViewState.Open:
                    _opened.Raise();
                    break;
                case ViewState.Opening:
                    _opening.Raise();
                    break;
                case ViewState.Closed:
                    _closed.Raise();
                    break;
                case ViewState.Closing:
                    _closing.Raise();
                    break;
            }
        }

        #endregion
    }
}