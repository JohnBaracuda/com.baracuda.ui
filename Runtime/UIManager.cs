using Baracuda.Mediator.Injection;
using Baracuda.Mediator.Locks;
using Baracuda.Tools;
using Baracuda.Utilities;
using Baracuda.Utilities.Types;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Baracuda.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Data

        [Inject] [Debug] private readonly CanvasSystemSettings _settings;

        private readonly Dictionary<Type, UIComponent> _instances = new();
        private readonly Dictionary<Type, Func<UIComponent>> _provider = new();
        private readonly Dictionary<Type, Action<UIComponent>> _disposer = new();
        private readonly Dictionary<Type, Func<Task<UIComponent>>> _asyncProvider = new();
        private readonly Dictionary<Type, Action<UIComponent>> _asyncDisposer = new();
        private readonly HashSet<Type> _asyncLocks = new();

        private readonly Dictionary<(Type, Type), Delegate> _transitionOverrides;

        [ReadonlyInspector]
        private List<UIComponent> Stack => UIStack.ToList();

        [ReadonlyInspector]
        private List<IReturnConsumer> ReturnConsumeStack => ReturnConsumerStack.ToList();

        public IEnumerable<UIComponent> Instances => _instances.Values;

        #endregion


        #region Initialization & Shutdown

        private void Start()
        {
            _settings.ReturnInput.action.performed -= OnReturnPressed;
            _settings.ReturnInput.action.performed += OnReturnPressed;
        }

        private void OnDestroy()
        {
            ReturnConsumerStack.Clear();
            UIStack.Clear();
            _settings.ReturnInput.action.performed -= OnReturnPressed;
        }

        #endregion


        #region Public API

        public Lock UILocks { get; } = new();
        public Lock ReturnLocks { get; } = new();
        public StackList<IReturnConsumer> ReturnConsumerStack { get; } = new();
        public StackList<UIComponent> UIStack { get; } = new();

        public async UniTask CloseAllWindowsAsync()
        {
            await CloseAllWindowsAsyncInternal();
        }

        public void CloseAllWindowsImmediate()
        {
            CloseAllWindowsImmediateInternal();
        }

        public void Preload<T>() where T : UIComponent
        {
            LoadInternal<T>().Forget();
        }

        public async UniTask PreloadAsync<T>() where T : UIComponent
        {
            await LoadInternal<T>();
        }

        public void Unload<T>() where T : UIComponent
        {
            UnloadInternal<T>();
        }

        public void Open<T>(T instance = null) where T : UIComponent
        {
            OpenInternal(instance).Forget();
        }

        public async UniTask<T> OpenAsync<T>(T instance = null) where T : UIComponent
        {
            return await OpenInternal(instance);
        }

        public void Close<T>(T instance = null) where T : UIComponent
        {
            CloseInternal(instance).Forget();
        }

        public async UniTask CloseAsync<T>(T instance = null) where T : UIComponent
        {
            await CloseInternal(instance);
        }

        public void Register<T>(T instance = null) where T : UIComponent
        {
            RegisterInternal(typeof(T), instance);
        }

        public void Register(Type type, UIComponent instance = null)
        {
            RegisterInternal(type, instance);
        }

        public void Register<T>(Func<T> provider, Action<UIComponent> disposer)
            where T : UIComponent
        {
            RegisterProviderInternal(provider, disposer);
        }

        public void Register<T>(Func<AsyncOperationHandle<T>> provider, Action<UIComponent> disposer,
            bool preload = true)
            where T : UIComponent
        {
            RegisterAsyncProviderInternal(provider, disposer, preload);
        }

        public void Unregister(Type type, UIComponent instance = null)
        {
            UnregisterInternal(type, instance);
        }

        #endregion


        #region OpenAsync

        private async UniTask<T> OpenInternal<T>(T instance = null) where T : UIComponent
        {
            if (instance != null)
            {
                await instance.OpenAsync();
                return instance;
            }

            if (_instances.TryGetValue(typeof(T), out var component))
            {
                await component.OpenAsync();
                return (T) component;
            }

            instance = await LoadInternal<T>();
            await instance.OpenAsync();
            return instance;

            /*
             * Begin consume escape
             * Remove Focus from active UI
             * Start Close Sequence for active UI
             * Start OpenAsync Sequence for new UI
             * End consume escape
             */
        }

        #endregion


        #region Close

        private async UniTask CloseInternal<T>(T instance = null) where T : UIComponent
        {
            if (instance != null)
            {
                await instance.CloseAsync();
                return;
            }

            if (_instances.TryGetValue(typeof(T), out var component))
            {
                await component.CloseAsync();
                return;
            }

            if (_asyncLocks.Contains(typeof(T)))
            {
                Debug.LogWarning("UI", $"An async operation is currently locking {typeof(T).Name}");
            }
        }

        #endregion


        #region Loading

        private async UniTask<T> LoadInternal<T>() where T : UIComponent
        {
            var key = typeof(T);

            if (_asyncLocks.Contains(key))
            {
                Debug.LogWarning("UI", $"An async operation is currently locking {key.Name}");
                return null;
            }

            if (_instances.TryGetValue(key, out var instance))
            {
                return (T) instance;
            }

            if (_provider.TryGetValue(key, out var providerFunc))
            {
                instance = providerFunc();
                UpdateSiblingIndex(instance);
                _instances.Add(key, instance);
                return (T) instance;
            }

            if (_asyncProvider.TryGetValue(key, out var asyncProviderFunc))
            {
                _asyncLocks.Add(key);
                instance = await asyncProviderFunc();
                UpdateSiblingIndex(instance);
                _instances.Add(key, instance);
                _asyncLocks.Remove(key);
                return (T) instance;
            }

            return null;
        }

        private void UnloadInternal<T>() where T : UIComponent
        {
            var key = typeof(T);

            if (_asyncLocks.Contains(key))
            {
                Debug.LogWarning("UI", $"An async operation is currently locking {key.Name}");
                return;
            }

            if (!_instances.TryRemove(key, out var instance))
            {
                return;
            }

            if (_disposer.TryGetValue(key, out var disposerAction))
            {
                disposerAction(instance);
                return;
            }

            if (_asyncDisposer.TryGetValue(key, out disposerAction))
            {
                disposerAction(instance);
                return;
            }

            Destroy(instance);
        }

        #endregion


        #region UI Registration

        private void RegisterInternal(Type type, UIComponent instance)
        {
            _instances.TryAdd(type, instance);
        }

        private void RegisterProviderInternal<T>(Func<T> provider, Action<UIComponent> disposer)
            where T : UIComponent
        {
            _provider.Add(typeof(T), provider);
            _disposer.Add(typeof(T), disposer);
        }

        private void RegisterAsyncProviderInternal<T>(Func<AsyncOperationHandle<T>> provider,
            Action<UIComponent> disposer,
            bool preload)
            where T : UIComponent
        {
            _asyncProvider.Add(typeof(T), async () =>
            {
                var result = await provider().Task;
                return result;
            });

            _asyncDisposer.Add(typeof(T), disposer);

            if (preload)
            {
                Preload<T>();
            }
        }

        private void UnregisterInternal(Type type, UIComponent instance = null)
        {
            _instances.Remove(type);
        }

        #endregion


        #region Window Handling

        private void CloseAllWindowsImmediateInternal()
        {
            while (UIStack.Any())
            {
                UIStack.Peek().CloseImmediate();
            }
        }

        private async UniTask CloseAllWindowsAsyncInternal()
        {
            var buffer = new Stack<UIComponent>(UIStack);
            UIStack.Clear();
            while (buffer.TryPop(out var element))
            {
                if (element.IsVisible)
                {
                    Debug.Log("UI", $"Closing Async {element}");
                    await element.CloseAsync();
                    Debug.Log("UI", $"Closed Async {element}");
                }
                else
                {
                    Debug.Log("UI", $"Closing Immediate {element}");
                    element.CloseImmediate();
                }
            }
        }

        #endregion


        #region Return Pressed

        private void OnReturnPressed(InputAction.CallbackContext context)
        {
            if (ReturnLocks.HasAny())
            {
                return;
            }
            if (UIComponent.TransitionSequence.IsActive())
            {
                UIComponent.TransitionSequence.Complete(true);
                return;
            }
            foreach (var consumer in ReturnConsumerStack.Reverse())
            {
                if (consumer.TryConsumeReturn())
                {
                    break;
                }
            }
        }

        public void LockReturnConsume(object dummy)
        {
            ReturnLocks.Add(dummy);
        }

        public async void UnlockReturnConsume(object dummy)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            ReturnLocks.Remove(dummy);
        }

        #endregion


        #region Misc

        private void UpdateSiblingIndex(UIComponent uiComponent)
        {
            var siblingIndex = _instances.FirstOrDefault().Value?.transform.GetSiblingIndex() ?? 0;
            uiComponent.transform.SetSiblingIndex(siblingIndex);
        }

        #endregion
    }
}