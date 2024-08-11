using System;
using System.Collections.Generic;
using Baracuda.Bedrock.Services;
using Baracuda.Utilities.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.UI
{
    public partial class UIManager
    {
        #region Fields

        private static UIManager Implementation { get; set; }

        private UISettings _settings;

        private readonly Dictionary<UIGroup, UIGroupManager> _groups = new();

        private UIContainer _container;

        #endregion


        #region Setup

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Implementation = null;
        }

        private void Awake()
        {
            ServiceLocator.Inject(ref _settings);

            Implementation = this;
            _container = gameObject.AddComponent<UIContainer>();

            foreach (var group in UIGroup.Registry.AllGroups())
            {
                var groupObject = new GameObject(group.ToString());
                groupObject.transform.SetParent(gameObject.transform);
                var groupManager = groupObject.AddComponent<UIGroupManager>();
                var groupSettings = _settings.GroupSettings.GetValueOrDefault(group);
                groupManager.Initialize(group, Container, groupSettings);
                _groups.Add(group, groupManager);
            }
        }

        #endregion


        #region Command Processing

        private async UniTask<IWindow> ProcessCommandInternal(UICommand command)
        {
            return command.CommandType switch
            {
                UICommandType.Load => await ProcessLoadCommandInternal(command),
                UICommandType.Unload => await ProcessUnloadCommandInternal(command),
                UICommandType.Open => await ProcessVisibilityCommandInternal(command),
                UICommandType.Close => await ProcessVisibilityCommandInternal(command),
                UICommandType.Toggle => await ProcessVisibilityCommandInternal(command),
                UICommandType.Focus => await ProcessVisibilityCommandInternal(command),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async UniTask<IWindow> ProcessVisibilityCommandInternal(UICommand command)
        {
            if (command.Window == null)
            {
                var window = Container.Get(command.WindowType) ?? await Container.LoadAsync(command.WindowType);
                command.WithInstance(window);
                Assert.IsNotNull(window, $"Window is null, no window of type {command.WindowType.Name} could be loaded!");
            }

            if (!command.Group.IsValid)
            {
                var groupType = command.Window?.GetDefaultGroup() ?? UIGroup.HUD;
                command.WithGroup(groupType);
            }

            var uiGroupManager = _groups[command.Group];
            return await uiGroupManager.ProcessVisibilityCommand(command);
        }

        private async UniTask<IWindow> ProcessUnloadCommandInternal(UICommand command)
        {
            Assert.IsTrue(command.CommandType == UICommandType.Unload);

            if (!command.Group.IsValid)
            {
                var groupType = command.Window?.GetDefaultGroup() ?? UIGroup.HUD;
                command.WithGroup(groupType);
            }

            var groupManager = _groups[command.Group];
            var window = await groupManager.ProcessUnloadCommand(command);
            return window;
        }

        private async UniTask<IWindow> ProcessLoadCommandInternal(UICommand command)
        {
            Assert.IsTrue(command.CommandType == UICommandType.Load);

            if (!command.Group.IsValid)
            {
                var groupType = command.Window?.GetDefaultGroup() ?? UIGroup.HUD;
                command.WithGroup(groupType);
            }

            var groupManager = _groups[command.Group];
            var window = await groupManager.ProcessLoadCommand(command);
            return window;
        }

        #endregion


        #region Internal Implementation

        private void LateUpdate()
        {
            foreach (var group in _groups.Values)
            {
                group.UpdateCommandQueue();
            }
        }

        private void CloseAllWindowsImmediateInternal()
        {
            foreach (var group in _groups.Values)
            {
                group.CloseAllWindowsImmediate();
            }
        }

        private async UniTask CloseAllWindowsAsyncInternal(CloseMode closeMode)
        {
            using var closeTasks = Buffer<UniTask>.Create();

            foreach (var group in _groups.Values)
            {
                closeTasks.Add(group.CloseAllWindowsAsync(closeMode));
            }

            await UniTask.WhenAll(closeTasks);
        }

        private bool IsOpenInAnyGroup<T>(T instance) where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOpen<T>(instance))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOpenInAnyGroup<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOpen<T>())
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOrWillOpenInAnyGroup<T>(T instance) where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOrWillOpen<T>(instance))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOrWillOpenInAnyGroup<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOrWillOpen<T>())
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsClosedInAllGroups<T>(T instance) where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsClosed<T>(instance))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsClosedInAllGroups<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOrWillOpen<T>())
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsOrWillCloseInAnyGroup<T>(T instance) where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOrWillClose<T>(instance))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOrWillCloseInAnyGroup<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var group in _groups.Values)
            {
                if (group.IsOrWillClose<T>())
                {
                    return true;
                }
            }
            return false;
        }

        private void FlushCommandsInternal()
        {
            foreach (var group in _groups.Values)
            {
                group.FlushCommandQueue();
            }
        }

        private void ClearCommandQueueInternal()
        {
            foreach (var group in _groups.Values)
            {
                group.ClearCommandQueue();
            }
        }

        #endregion
    }
}