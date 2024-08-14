using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Odin;
using Baracuda.Bedrock.Services;
using Baracuda.UI.Components;
using Baracuda.Utilities;
using Baracuda.Utilities.Collections;
using Baracuda.Utilities.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Baracuda.UI
{
    public partial class UIGroupManager : MonoBehaviour
    {
        #region Fields

        [ReadonlyInspector]
        private List<IWindow> Stack => _uiStack.List;

        [Debug]
        private UIGroup _group;
        [Debug]
        private UIGroupSettings _settings;
        [Debug]
        private UIBackground _background;
        private bool _hasGroupBackground;
        private UIContainer _container;
        private Sequence _transitionSequence;
        private readonly PriorityQueue<(UICommand command, UniTaskCompletionSource<IWindow> completionSource)> _queue = new();
        private readonly StackList<IWindow> _uiStack = new();
        private readonly Broadcast<IWindow, IWindow> _transitionStarted = new();
        private readonly Broadcast<IWindow, IWindow> _transitionCompleted = new();
        private readonly Broadcast<IWindow> _openSequenceStarted = new();
        private readonly Broadcast<IWindow> _openSequenceCompleted = new();
        private readonly Broadcast<IWindow> _closeSequenceStarted = new();
        private readonly Broadcast<IWindow> _closeSequenceCompleted = new();
        private Func<EscapeUsage> _escapeDelegate;
        private int SortingOrder => _settings.sortingOrder;
        private bool ConsumeEscape => _settings.consumeEscape;

        public void Initialize(UIGroup group, UIContainer container, UIGroupSettings settings)
        {
            _group = group;
            _container = container;
            _settings = settings;
            _escapeDelegate = OnEscapePressed;
            if (settings.hasBackground && settings.background)
            {
                _background = Instantiate(settings.background);
                _background.DontDestroyOnLoad();
                _hasGroupBackground = true;
#if UNITY_EDITOR
                UnityEditor.SceneVisibilityManager.instance.Hide(_background.gameObject, true);
#endif
            }
        }

        #endregion


        #region Internal API

        internal void CloseAllWindowsImmediate()
        {
            FlushCommandQueue();

            var context = UIContext.Create()
                .Flush()
                .Build();

            while (_uiStack.TryPeek(out var window))
            {
                ExecuteWindowClosingCallbacks(window);
                ExecuteWindowFocusLossCallbacks(window);
                _uiStack.Remove(window);
                window.HideAsync(context).Complete(true);
            }
        }

        internal async UniTask CloseAllWindowsAsync(CloseMode closeMode)
        {
            if (_transitionSequence.IsActive())
            {
                FlushCommandQueue();
            }

            var context = UIContext.Create()
                .Flush()
                .Build();

            switch (closeMode)
            {
                case CloseMode.Sequential:
                {
                    while (_uiStack.TryPeek(out var window))
                    {
                        ExecuteWindowClosingCallbacks(window);
                        ExecuteWindowFocusLossCallbacks(window);
                        _uiStack.Remove(window);
                        await window.HideAsync(context).AsyncWaitForCompletion();
                        ExecuteWindowClosedCallbacks(window);
                        SetActive(window, false);
                    }
                    ResetEscapeForTransition();
                    return;
                }

                case CloseMode.Parallel:
                {
                    using var tasks = Buffer<Task>.Create();

                    while (_uiStack.TryPeek(out var window))
                    {
                        ExecuteWindowClosingCallbacks(window);
                        ExecuteWindowFocusLossCallbacks(window);
                        _uiStack.Remove(window);
                        var sequence = window.HideAsync(context);
                        sequence.OnComplete(() =>
                        {
                            SetActive(window, false);
                            ExecuteWindowClosedCallbacks(window);
                        });
                        tasks.Add(sequence.AsyncWaitForCompletion());
                    }

                    await Task.WhenAll(tasks);
                    ResetEscapeForTransition();
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(closeMode), closeMode, null);
            }
        }

        internal void FlushCommandQueue()
        {
            _transitionSequence?.Complete(true);

            if (_queue.IsEmpty)
            {
                return;
            }

            Debug.Log("UI", $"Flushing [{_queue.Count}] Commands in [{_group}]");

            while (_queue.TryDequeue(out var scheduledCommand))
            {
                ExecuteCommandInternal(scheduledCommand.command);
                _transitionSequence?.Complete(true);
            }
            ResetEscapeForTransition();
        }

        internal void ClearCommandQueue()
        {
            while (_queue.TryDequeue(out var scheduledCommand))
            {
                scheduledCommand.completionSource.TrySetCanceled();
            }
        }

        #endregion


        #region Process Command

        internal async UniTask<IWindow> ProcessUnloadCommand(UICommand command)
        {
            Assert.IsTrue(command.Group == _group);
            Assert.IsTrue(command.CommandType == UICommandType.Unload);

            if (_transitionSequence.IsActive())
            {
                if (command.ExecuteImmediate)
                {
                    FlushCommandQueue();
                }

                var completionSource = new UniTaskCompletionSource<IWindow>();
                _queue.Enqueue((command, completionSource), command.Priority);
                return await completionSource.Task;
            }

            return await UnloadAsyncInternal(command);
        }

        internal async UniTask<IWindow> ProcessLoadCommand(UICommand command)
        {
            Assert.IsTrue(command.Group == _group);
            Assert.IsTrue(command.CommandType == UICommandType.Load);

            return await LoadAsyncInternal(command);
        }

        internal async UniTask<IWindow> ProcessVisibilityCommand(UICommand command)
        {
            Assert.IsTrue(command.Group == _group);
            Assert.IsTrue(command.CommandType != UICommandType.Unload);
            Assert.IsTrue(command.CommandType != UICommandType.Load);

            if (command.ExecuteImmediate)
            {
                FlushCommandQueue();
            }

            if (_transitionSequence.IsActive())
            {
                var completionSource = new UniTaskCompletionSource<IWindow>();
                _queue.Enqueue((command, completionSource), command.Priority);
                return await completionSource.Task;
            }

            return await ExecuteCommandInternal(command);
        }

        private UniTask<IWindow> ExecuteCommandInternal(UICommand command)
        {
            return command.CommandType switch
            {
                UICommandType.Open => OpenAsyncInternal(command),
                UICommandType.Close => CloseAsyncInternal(command),
                UICommandType.Toggle => ToggleAsyncInternal(command),
                UICommandType.Focus => FocusAsyncInternal(command),
                UICommandType.Load => LoadAsyncInternal(command),
                UICommandType.Unload => UnloadAsyncInternal(command),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion


        #region Focus Command

        private async UniTask<IWindow> FocusAsyncInternal(UICommand command)
        {
            Assert.IsTrue(command.Group == _group);
            var window = command.Window ?? await _container.LoadAsync(command.WindowType);

            if (IsOnTopOfStack(window))
            {
                return window;
            }

            if (_uiStack.Count < 2)
            {
                return window;
            }

            if (_uiStack.List.Contains(window) is false)
            {
                return window;
            }

            var topWindow = _uiStack.Peek();
            ExecuteWindowFocusLossCallbacks(topWindow);
            _uiStack.PushUnique(window);
            ExecuteWindowFocusGainCallbacks(window);

            UpdateWindowSortingOrder();
            return window;
        }

        #endregion


        #region Open Command

        private async UniTask<IWindow> OpenAsyncInternal(UICommand command)
        {
            Assert.IsFalse(_transitionSequence.IsActive());
            Assert.IsTrue(command.Group == _group);

            var window = command.Window ?? await _container.LoadAsync(command.WindowType);
            var previousWindow = _uiStack.Peek();
            var isFirstWindow = previousWindow == null;
            var windowTransitionSettings = GetTransitionSettingsForWindow(window);
            var previousWindowTransitionSettings = GetTransitionSettingsForWindow(previousWindow);
            var hideUnderlyingUI = !isFirstWindow && (windowTransitionSettings & TransitionSettings.HideUnderlyingWindows) > 0;
            var completeTransitionsSequential = (windowTransitionSettings & TransitionSettings.CompleteTransitionsSequential) > 0;
            var hideOnFocusLoss = previousWindow != null && (previousWindowTransitionSettings & TransitionSettings.HideWhenLosingFocus) > 0;

            var context = UIContext.Create()
                .FromWindow(previousWindow)
                .ToWindow(window)
                .Build();

            _transitionStarted.Raise(previousWindow, window);

            ExecuteWindowOpeningCallbacks(window);
            AddWindowToStack(window, command);

            _transitionSequence = DOTween.Sequence(this);
            _transitionSequence.SetRecyclable(false);

            SetActive(window, true);
            var hasLostFocus = hideOnFocusLoss && previousWindow != _uiStack.Peek();
            if (hideUnderlyingUI || hasLostFocus)
            {
                var hideSequence = previousWindow.HideAsync(context);
                _transitionSequence.Append(hideSequence);
            }

            var showSequence = window.ShowAsync(context);
            if (completeTransitionsSequential)
            {
                _transitionSequence.Append(showSequence);
            }
            else
            {
                _transitionSequence.Join(showSequence);
            }

            _transitionSequence.OnComplete(() =>
            {
                ExecuteWindowOpenedCallbacks(window);
                _transitionCompleted.Raise(previousWindow, window);
                _transitionSequence = null;
                ResetEscapeForTransition();
            });

            SetupEscapeForTransition();
            await _transitionSequence.AsyncWaitForCompletion();
            return window;
        }

        #endregion


        #region Close Command

        private async UniTask<IWindow> CloseAsyncInternal(UICommand command)
        {
            if (IsCommandWindowOpen(command) is false)
            {
                return command.Window ?? _container.Get(command.WindowType);
            }

            Assert.IsTrue(IsCommandWindowOpen(command));
            Assert.IsTrue(!_transitionSequence.IsActive());
            Assert.IsTrue(command.Group == _group);

            var window = command.Window ?? _container.Get(command.WindowType);

            ExecuteWindowClosingCallbacks(window);
            RemoveWindowFromStack(window);

            var refocusWindow = _uiStack.Peek();
            var transitionSettings = GetTransitionSettingsForWindow(refocusWindow);
            var wasWindowHidden = (GetTransitionSettingsForWindow(window) & TransitionSettings.HideUnderlyingWindows) != 0;
            var showWindowAgain = refocusWindow != null && (wasWindowHidden || (transitionSettings & TransitionSettings.HideWhenLosingFocus) > 0);

            var context = UIContext.Create()
                .FromWindow(window)
                .ToWindow(refocusWindow)
                .Build();

            _transitionStarted.Raise(window, refocusWindow);
            _transitionSequence = DOTween.Sequence(this);
            _transitionSequence.SetRecyclable(false);

            var hideSequence = window.HideAsync(context);
            _transitionSequence.Append(hideSequence);

            if (showWindowAgain)
            {
                var showSequence = refocusWindow.ShowAsync(context);
                _transitionSequence.Join(showSequence);
            }

            _transitionSequence.OnComplete(() =>
            {
                ExecuteWindowClosedCallbacks(window);
                SetActive(window, false);
                _transitionCompleted.Raise(window, _uiStack.Peek());
                _transitionSequence = null;
                ResetEscapeForTransition();
            });

            SetupEscapeForTransition();
            await _transitionSequence.AsyncWaitForCompletion();
            return window;
        }

        #endregion


        #region Toggle Command

        private async UniTask<IWindow> ToggleAsyncInternal(UICommand command)
        {
            if (IsCommandWindowOpen(command))
            {
                return await CloseAsyncInternal(command);
            }
            return await OpenAsyncInternal(command);
        }

        #endregion


        #region Load Command

        private async UniTask<IWindow> LoadAsyncInternal(UICommand command)
        {
            var window = command.Window ?? await _container.LoadAsync(command.WindowType);
            if (!IsCommandWindowOpen(command))
            {
                SetActive(window, false);
            }
            return window;
        }

        #endregion


        #region Unload Command

        private async UniTask<IWindow> UnloadAsyncInternal(UICommand command)
        {
            var window = command.Window ?? _container.Get(command.WindowType);
            if (IsCommandWindowOpen(command))
            {
                await CloseAsyncInternal(command);
            }
            _container.Unload(window);
            return window;
        }

        #endregion


        #region Escaple Handling

        private void SetupEscapeForTransition()
        {
            if (!ConsumeEscape)
            {
                return;
            }
            var inputManager = ServiceLocator.Get<InputManager>();
            inputManager.AddEscapeConsumer(_escapeDelegate);
        }

        private void ResetEscapeForTransition()
        {
            if (!ConsumeEscape)
            {
                return;
            }
            var inputManager = ServiceLocator.Get<InputManager>();
            inputManager.RemoveEscapeConsumer(_escapeDelegate);
        }

        private EscapeUsage OnEscapePressed()
        {
            if (_transitionSequence.IsActive())
            {
                _transitionSequence.Complete(true);
                return EscapeUsage.ConsumedEscape;
            }
            var inputManager = ServiceLocator.Get<InputManager>();
            inputManager.RemoveEscapeConsumer(_escapeDelegate);
            return EscapeUsage.IgnoredEscape;
        }

        #endregion


        #region UI Stack

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddWindowToStack(IWindow window, UICommand command)
        {
            if (_hasGroupBackground && _uiStack.Count == 0 && _backgroundBlocker.Count <= 0)
            {
                _background.Show();
            }

            var previousFocusWindow = _uiStack.Peek();

            _uiStack.PushUnique(window);

            ProcessBellowTypePlacement(window, command.BellowTypes);
            ProcessAboveTypePlacement(window, command.AboveTypes);

            if (IsOnTopOfStack(window))
            {
                ExecuteWindowFocusLossCallbacks(previousFocusWindow);
                ExecuteWindowFocusGainCallbacks(window);
            }

            UpdateWindowSortingOrder();
        }

        private void UpdateWindowSortingOrder()
        {
            if (_hasGroupBackground)
            {
                _background.SetSortingOrder(SortingOrder);
            }
            for (var index = 0; index < _uiStack.List.Count; index++)
            {
                var sortingOrder = SortingOrder + (index + 1) * 100;
                _uiStack[index].SetSortingOrder(sortingOrder);
            }
        }

        private void ProcessAboveTypePlacement(IWindow window, List<Type> aboveTypes)
        {
            if (aboveTypes.Count == 0)
            {
                return;
            }

            var stackList = _uiStack.List;
            var targetIndex = stackList.IndexOf(window);

            for (var index = stackList.Count - 1; index >= 0; index--)
            {
                var elementAtIndex = stackList[index];
                if (aboveTypes.Contains(elementAtIndex.GetType()) && targetIndex < index)
                {
                    targetIndex = index + 1;
                    targetIndex.RefWithMaxLimit(stackList.Count - 1);
                }
            }

            stackList.Remove(window);
            stackList.Insert(targetIndex, window);
        }

        private void ProcessBellowTypePlacement(IWindow window, List<Type> bellowTypes)
        {
            if (bellowTypes.Count == 0)
            {
                return;
            }

            var stackList = _uiStack.List;
            var targetIndex = stackList.IndexOf(window);

            for (var index = 0; index < stackList.Count; index++)
            {
                var elementAtIndex = stackList[index];
                if (bellowTypes.Contains(elementAtIndex.GetType()) && targetIndex > index)
                {
                    targetIndex = index - 1;
                    targetIndex.RefWithMinLimit(0);
                }
            }

            stackList.Remove(window);
            stackList.Insert(targetIndex, window);
        }

        private void RemoveWindowFromStack(IWindow window)
        {
            if (IsOnTopOfStack(window))
            {
                ExecuteWindowFocusLossCallbacks(window);
                _uiStack.Remove(window);
                ExecuteWindowFocusGainCallbacks(_uiStack.Peek());
            }
            else
            {
                _uiStack.Remove(window);
            }

            if (_hasGroupBackground && _uiStack.Count == 0)
            {
                _background.Hide();
            }

            UpdateWindowSortingOrder();
        }

        private bool IsOnTopOfStack(IWindow window)
        {
            return window != null && AreEqual(window, _uiStack.Peek());
        }

        public bool IsOpen<T>(IWindow instance) where T : MonoBehaviour, IWindow
        {
            if (instance != null)
            {
                return _uiStack.Contains(instance);
            }
            return IsOpen<T>();
        }

        public bool IsOpen<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var window in _uiStack)
            {
                if (window is T)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOrWillOpen<T>(IWindow instance) where T : MonoBehaviour, IWindow
        {
            var isOpen = IsOpen<T>(instance);
            if (isOpen)
            {
                return true;
            }

            foreach (var (command, _) in _queue)
            {
                var isOpenCommand = command.CommandType is UICommandType.Open or UICommandType.Toggle;
                if (isOpenCommand && instance == command.Window)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOrWillOpen<T>() where T : MonoBehaviour, IWindow
        {
            var isOpen = IsOpen<T>();
            if (isOpen)
            {
                return true;
            }

            foreach (var (command, _) in _queue)
            {
                if (command.CommandType is UICommandType.Open or UICommandType.Toggle)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCommandWindowOpen(UICommand command)
        {
            if (command.Window != null)
            {
                var isOpen = _uiStack.Contains(command.Window);
                return isOpen;
            }

            foreach (var element in _uiStack)
            {
                if (element.GetType() == command.WindowType)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Is Or Will Close

        public bool IsClosed<T>(IWindow instance) where T : MonoBehaviour, IWindow
        {
            if (instance != null)
            {
                return !_uiStack.Contains(instance);
            }
            return IsClosed<T>();
        }

        public bool IsClosed<T>() where T : MonoBehaviour, IWindow
        {
            foreach (var window in _uiStack)
            {
                if (window is T)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsOrWillClose<T>(IWindow instance) where T : MonoBehaviour, IWindow
        {
            var isClosed = IsClosed<T>(instance);
            if (isClosed)
            {
                return true;
            }

            foreach (var (command, _) in _queue)
            {
                var isCloseCommand = command.CommandType is UICommandType.Close or UICommandType.Toggle;
                if (isCloseCommand && instance == command.Window)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOrWillClose<T>() where T : MonoBehaviour, IWindow
        {
            var isClosed = IsClosed<T>();
            if (isClosed)
            {
                return true;
            }

            foreach (var (command, _) in _queue)
            {
                if (command.CommandType is UICommandType.Close or UICommandType.Toggle)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion


        #region Transition Methods

        internal async void UpdateCommandQueue()
        {
            if (_transitionSequence.IsActive())
            {
                return;
            }
            if (!_queue.TryDequeue(out var value))
            {
                return;
            }

            var command = value.command;
            var completionSource = value.completionSource;
            var result = await ExecuteCommandInternal(command);
            completionSource.TrySetResult(result);
        }

        #endregion


        #region Window Callbacks

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowOpeningCallbacks(IWindow window)
        {
            _openSequenceStarted.Raise(window);

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowOpening>())
            {
                windowOpening.OnWindowOpening();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowOpenedCallbacks(IWindow window)
        {
            _openSequenceCompleted.Raise(window);

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowOpened>())
            {
                windowOpening.OnWindowOpened();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowClosingCallbacks(IWindow window)
        {
            _closeSequenceStarted.Raise(window);

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowClosing>())
            {
                windowOpening.OnWindowClosing();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowClosedCallbacks(IWindow window)
        {
            _closeSequenceCompleted.Raise(window);

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowClosed>())
            {
                windowOpening.OnWindowClosed();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowFocusGainCallbacks(IWindow window)
        {
            if (window is IWindowEscapeHandler windowEscapeHandler)
            {
                var inputManager = ServiceLocator.Get<InputManager>();
                inputManager.AddEscapeConsumer(windowEscapeHandler.OnEscapePressed);
            }

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowFocusGain>())
            {
                windowOpening.OnWindowGainedFocus();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteWindowFocusLossCallbacks(IWindow window)
        {
            if (window is IWindowEscapeHandler windowEscapeHandler)
            {
                var inputManager = ServiceLocator.Get<InputManager>();
                inputManager.RemoveEscapeConsumer(windowEscapeHandler.OnEscapePressed);
            }

            if (window is not MonoBehaviour monoBehaviour)
            {
                return;
            }
            foreach (var windowOpening in monoBehaviour.GetComponents<IWindowFocusLost>())
            {
                windowOpening.OnWindowLostFocus();
            }
        }

        #endregion


        #region Background Blocking

        [Debug]
        private HashSet<Object> _backgroundBlocker = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlockBackgroundInternal(Object source)
        {
            if (!_hasGroupBackground)
            {
                return;
            }
            _backgroundBlocker.Add(source);
            _background.Hide();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnblockBackgroundInternal(Object source)
        {
            if (!_hasGroupBackground)
            {
                return;
            }
            _backgroundBlocker.Remove(source);
            if (_backgroundBlocker.Count == 0 && _uiStack.Count > 0)
            {
                _background.Show();
            }
        }

        #endregion


        #region Helper

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AreEqual(IWindow rhs, IWindow lhs)
        {
            return EqualityComparer<IWindow>.Default.Equals(rhs, lhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TransitionSettings GetTransitionSettingsForWindow(IWindow window)
        {
            if (window is IWindowTransitionSettings transitionSettings)
            {
                return transitionSettings.TransitionSettings;
            }

            if (window is MonoBehaviour monoBehaviour && monoBehaviour.TryGetComponent(out transitionSettings))
            {
                return transitionSettings.TransitionSettings;
            }

            return TransitionSettings.None;
        }

        private void SetActive(IWindow window, bool value)
        {
            if (window is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.SetActive(value);
            }
        }

        #endregion
    }
}