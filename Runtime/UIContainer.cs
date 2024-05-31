using Baracuda.Bedrock.Odin;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Baracuda.UI
{
    public class UIContainer : MonoBehaviour, IDisposable
    {
        #region Fields

        [ReadonlyInspector]
        private readonly Dictionary<Type, UIWindow> _instances = new();
        [ReadonlyInspector]
        private readonly Dictionary<Type, Func<UIWindow>> _provider = new();
        [ReadonlyInspector]
        private readonly Dictionary<Type, Action<UIWindow>> _disposer = new();
        [ReadonlyInspector]
        private readonly Dictionary<Type, Func<Task<UIWindow>>> _asyncProvider = new();
        [ReadonlyInspector]
        private readonly Dictionary<Type, Action<UIWindow>> _asyncDisposer = new();
        [ReadonlyInspector]
        private readonly HashSet<Type> _asyncLocks = new();

        #endregion


        #region Public

        public IEnumerable<UIWindow> LoadedInstances => _instances.Values;

        #endregion


        #region Add

        public void Add<T>(T instance = null) where T : UIWindow
        {
            AddInternal(typeof(T), instance);
        }

        public void Add(Type type, UIWindow instance = null)
        {
            AddInternal(type, instance);
        }

        public void Add<T>(Func<T> provider, Action<UIWindow> disposer)
            where T : UIWindow
        {
            AddProviderInternal(provider, disposer);
        }

        public void Add<T>(Func<AsyncOperationHandle<T>> provider, Action<UIWindow> disposer,
            bool preload = true)
            where T : UIWindow
        {
            AddAsyncProviderInternal(provider, disposer, preload);
        }

        #endregion


        #region Remove

        public void Remove(Type type, UIWindow instance = null)
        {
            RemoveInternal(type, instance);
        }

        #endregion


        #region GetIfLoaded

        public bool IsInstanceLoaded<T>(out T uiComponent) where T : UIWindow
        {
            if (_instances.TryGetValue(typeof(T), out var result))
            {
                uiComponent = (T) result;
                return true;
            }
            uiComponent = default(T);
            return false;
        }

        public T Get<T>() where T : UIWindow
        {
            var type = typeof(T);
            if (_instances.TryGetValue(type, out var result))
            {
                return (T) result;
            }
            if (_provider.TryGetValue(type, out var providerFunc))
            {
                var instance = providerFunc();
                UpdateSiblingIndex(instance);
                _instances.Add(type, instance);
                return (T) instance;
            }
            return default(T);
        }

        #endregion


        #region Loading

        public async UniTask<T> Load<T>() where T : UIWindow
        {
            return await LoadInternal<T>();
        }

        public void Unload<T>() where T : UIWindow
        {
            UnloadInternal<T>();
        }

        #endregion


        #region UI Registration

        private void AddInternal(Type type, UIWindow instance)
        {
            _instances.TryAdd(type, instance);
        }

        private void AddProviderInternal<T>(Func<T> provider, Action<UIWindow> disposer)
            where T : UIWindow
        {
            _provider.Add(typeof(T), provider);
            _disposer.Add(typeof(T), disposer);
        }

        private void AddAsyncProviderInternal<T>(Func<AsyncOperationHandle<T>> provider,
            Action<UIWindow> disposer,
            bool preload)
            where T : UIWindow
        {
            _asyncProvider.Add(typeof(T), async () =>
            {
                var result = await provider().Task;
                return result;
            });

            _asyncDisposer.Add(typeof(T), disposer);

            if (preload)
            {
                LoadInternal<T>().Forget();
            }
        }

        private void RemoveInternal(Type type, UIWindow instance = null)
        {
            _instances.Remove(type);
        }

        #endregion


        #region Loading & Unloading Internal

        private async UniTask<T> LoadInternal<T>() where T : UIWindow
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

        private void UnloadInternal<T>() where T : UIWindow
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


        #region Misc

        private void UpdateSiblingIndex(UIWindow uiWindow)
        {
            var siblingIndex = _instances.FirstOrDefault().Value?.transform.GetSiblingIndex() ?? 0;
            uiWindow.transform.SetSiblingIndex(siblingIndex);
        }

        private void OnDestroy()
        {
            _instances.Clear();
            _provider.Clear();
            _disposer.Clear();
            _asyncProvider.Clear();
            _asyncDisposer.Clear();
            _asyncLocks.Clear();
        }

        #endregion


        #region Dispose

        public void Dispose()
        {
            _instances.Clear();
            _provider.Clear();
            _disposer.Clear();
            _asyncProvider.Clear();
            _asyncDisposer.Clear();
            _asyncLocks.Clear();
        }

        #endregion
    }
}