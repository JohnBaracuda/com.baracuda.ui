using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baracuda.Utility.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Baracuda.UI
{
    public class UIContainer : MonoBehaviour, IDisposable
    {
        #region Fields

        private readonly Dictionary<Type, IWindow> _instances = new();
        private readonly Dictionary<Type, Func<IWindow>> _provider = new();
        private readonly Dictionary<Type, Action<MonoBehaviour>> _disposer = new();
        private readonly Dictionary<Type, Func<Task<IWindow>>> _asyncProvider = new();
        private readonly Dictionary<Type, Action<MonoBehaviour>> _asyncDisposer = new();
        private readonly HashSet<Type> _asyncLocks = new();

        #endregion


        #region Public

        public IEnumerable<IWindow> LoadedInstances => _instances.Values;

        #endregion


        #region Add

        public void Add<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            AddInternal(typeof(T), instance);
        }

        public void Add(Type type, IWindow instance = null)
        {
            AddInternal(type, instance);
        }

        public void Add<T>(Func<T> provider, Action<MonoBehaviour> disposer)
            where T : MonoBehaviour, IWindow
        {
            AddProviderInternal(provider, disposer);
        }

        public void Add<T>(Func<AsyncOperationHandle<T>> provider, Action<MonoBehaviour> disposer,
            bool preload = true)
            where T : MonoBehaviour, IWindow
        {
            AddAsyncProviderInternal(provider, disposer, preload);
        }

        #endregion


        #region Remove

        public void Remove(Type type, IWindow instance = null)
        {
            RemoveInternal(type, instance);
        }

        #endregion


        #region Get

        public bool IsLoaded<T>() where T : MonoBehaviour, IWindow
        {
            return _instances.ContainsKey(typeof(T));
        }

        public T Get<T>() where T : IWindow
        {
            return (T)Get(typeof(T));
        }

        public IWindow Get(Type type)
        {
            return _instances.GetValueOrDefault(type);
        }

        #endregion


        #region Loading

        public T Load<T>() where T : IWindow
        {
            return (T)Load(typeof(T));
        }

        public IWindow Load(Type type)
        {
            return LoadInternal(type);
        }

        public async UniTask<T> LoadAsync<T>() where T : MonoBehaviour, IWindow
        {
            return (T)await LoadAsyncInternal(typeof(T));
        }

        public async UniTask<IWindow> LoadAsync(Type type)
        {
            return await LoadAsyncInternal(type);
        }

        public void Unload<T>() where T : MonoBehaviour, IWindow
        {
            UnloadInternal<T>();
        }

        public void Unload<T>(T instance) where T : MonoBehaviour, IWindow
        {
            UnloadInternal(instance);
        }

        public void Unload(IWindow instance)
        {
            UnloadInternal(instance?.GetType());
        }

        #endregion


        #region UI Registration

        private void AddInternal(Type type, IWindow instance)
        {
            _instances.TryAdd(type, instance);
        }

        private void AddProviderInternal<T>(Func<T> provider, Action<MonoBehaviour> disposer)
            where T : MonoBehaviour, IWindow
        {
            _provider.Add(typeof(T), provider);
            _disposer.Add(typeof(T), disposer);
        }

        private void AddAsyncProviderInternal<T>(Func<AsyncOperationHandle<T>> provider,
            Action<MonoBehaviour> disposer,
            bool preload)
            where T : MonoBehaviour, IWindow
        {
            _asyncProvider.Add(typeof(T), async () =>
            {
                var result = await provider().Task;
                return result;
            });

            _asyncDisposer.Add(typeof(T), disposer);

            if (preload)
            {
                LoadAsyncInternal(typeof(T)).Forget();
            }
        }

        private void RemoveInternal(Type type, IWindow instance = null)
        {
            _instances.Remove(type);
        }

        #endregion


        #region Loading & Unloading Internal

        private IWindow LoadInternal(Type type)
        {
            if (_instances.TryGetValue(type, out var result))
            {
                return result;
            }
            if (_provider.TryGetValue(type, out var providerFunc))
            {
                var instance = providerFunc();
                UpdateSiblingIndex((MonoBehaviour)instance);
                _instances.Add(type, instance);
                return instance;
            }
            return default;
        }

        private async UniTask<IWindow> LoadAsyncInternal(Type key)
        {
            if (_asyncLocks.Contains(key))
            {
                Debug.LogWarning("UI", $"An async operation is currently locking {key.Name}");
                return null;
            }

            if (_instances.TryGetValue(key, out var instance))
            {
                return instance;
            }

            if (_provider.TryGetValue(key, out var providerFunc))
            {
                instance = providerFunc();
                UpdateSiblingIndex((MonoBehaviour)instance);
                _instances.Add(key, instance);
                return instance;
            }

            if (_asyncProvider.TryGetValue(key, out var asyncProviderFunc))
            {
                _asyncLocks.Add(key);
                instance = await asyncProviderFunc();
                UpdateSiblingIndex((MonoBehaviour)instance);
                _instances.Add(key, instance);
                _asyncLocks.Remove(key);
                return instance;
            }

            return null;
        }

        private void UnloadInternal<T>() where T : MonoBehaviour, IWindow
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
                disposerAction((MonoBehaviour)instance);
                return;
            }

            if (_asyncDisposer.TryGetValue(key, out disposerAction))
            {
                disposerAction((MonoBehaviour)instance);
                return;
            }

            Destroy(((MonoBehaviour)instance).gameObject);
        }

        private void UnloadInternal<T>(T instance) where T : MonoBehaviour, IWindow
        {
            Destroy(instance.gameObject);
        }

        private void UnloadInternal(Type type)
        {
            if (type is null)
            {
                return;
            }

            if (_asyncLocks.Contains(type))
            {
                Debug.LogWarning("UI", $"An async operation is currently locking {type.Name}");
                return;
            }

            if (!_instances.TryRemove(type, out var instance))
            {
                return;
            }

            if (_disposer.TryGetValue(type, out var disposerAction))
            {
                disposerAction((MonoBehaviour)instance);
                return;
            }

            if (_asyncDisposer.TryGetValue(type, out disposerAction))
            {
                disposerAction((MonoBehaviour)instance);
                return;
            }

            Destroy(((MonoBehaviour)instance).gameObject);
        }

        #endregion


        #region Misc

        private void UpdateSiblingIndex(MonoBehaviour uiWindow)
        {
            var instance = _instances.FirstOrDefault().Value;
            var monoBehaviour = (MonoBehaviour)instance;
            var siblingIndex = monoBehaviour.transform.GetSiblingIndex();
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