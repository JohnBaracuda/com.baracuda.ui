using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Bedrock.Events;
using Baracuda.Bedrock.Injection;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Locks;
using Baracuda.Bedrock.Odin;
using Baracuda.Utilities;
using Baracuda.Utilities.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        ///     Triggered when the open sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow> OpenSequenceStarted
        {
            add => _openSequenceStarted.Add(value);
            remove => _openSequenceStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the open sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow> OpenSequenceCompleted
        {
            add => _openSequenceCompleted.Add(value);
            remove => _openSequenceCompleted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow> CloseSequenceStarted
        {
            add => _closeSequenceStarted.Add(value);
            remove => _closeSequenceStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow> CloseSequenceCompleted
        {
            add => _closeSequenceCompleted.Add(value);
            remove => _closeSequenceCompleted.Remove(value);
        }

        /// <summary>
        ///     Triggered when a transition between two UI windows starts.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow, UIWindow> TransitionStarted
        {
            add => _transitionStarted.Add(value);
            remove => _transitionStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when a transition between two UI windows completes.
        /// </summary>
        [PublicAPI]
        public event Action<UIWindow, UIWindow> TransitionCompleted
        {
            add => _transitionCompleted.Add(value);
            remove => _transitionCompleted.Remove(value);
        }

        #endregion


        #region Public API

        /// <summary>
        ///     UI Container holding every available UI Prefab or provider method.
        /// </summary>
        [PublicAPI]
        public UIContainer Container { get; private set; }

        /// <summary>
        ///     Lock to disable UI interaction.
        /// </summary>
        [PublicAPI]
        public Lock UILocks { get; } = new();

        /// <summary>
        ///     Closes all windows asynchronously.
        /// </summary>
        [PublicAPI]
        public async UniTask CloseAllWindowsAsync()
        {
            await CloseAllWindowsAsyncInternal();
        }

        /// <summary>
        ///     Closes all windows immediately.
        /// </summary>
        [PublicAPI]
        public void CloseAllWindowsImmediate()
        {
            CloseAllWindowsImmediateInternal();
        }

        /// <summary>
        ///     Preloads the specified UI window.
        /// </summary>
        [PublicAPI]
        public void Preload<T>() where T : UIWindow
        {
            Container.Load<T>().Forget();
        }

        /// <summary>
        ///     Preloads the specified UI window asynchronously.
        /// </summary>
        [PublicAPI]
        public async UniTask<T> PreloadAsync<T>() where T : UIWindow
        {
            return await Container.Load<T>();
        }

        /// <summary>
        ///     Unloads the specified UI window.
        /// </summary>
        [PublicAPI]
        public void Unload<T>() where T : UIWindow
        {
            Container?.Unload<T>();
        }

        /// <summary>
        ///     Opens the specified UI window.
        /// </summary>
        [PublicAPI]
        public void Open<T>(T instance = null) where T : UIWindow
        {
            OpenInternal(instance).Forget();
        }

        /// <summary>
        ///     Opens the specified UI window once active transitions have been completed.
        /// </summary>
        [PublicAPI]
        public void OpenScheduled<T>(T instance = null) where T : UIWindow
        {
            OpenScheduledInternal<T>();
        }

        /// <summary>
        ///     Opens the specified UI window immediately.
        /// </summary>
        [PublicAPI]
        public void OpenImmediate<T>(T instance = null) where T : UIWindow
        {
            OpenImmediateInternal(instance);
        }

        /// <summary>
        ///     Opens the specified UI window asynchronously.
        /// </summary>
        [PublicAPI]
        public async UniTask<T> OpenAsync<T>(T instance = null) where T : UIWindow
        {
            return await OpenInternal(instance);
        }

        /// <summary>
        ///     Closes the specified UI window.
        /// </summary>
        [PublicAPI]
        public void Close<T>(T instance = null) where T : UIWindow
        {
            CloseInternal(instance).Forget();
        }

        /// <summary>
        ///     Closes the specified UI window immediately.
        /// </summary>
        [PublicAPI]
        public void CloseImmediate<T>(T instance = null) where T : UIWindow
        {
            CloseImmediateInternal(instance);
        }

        /// <summary>
        ///     Closes the specified UI window asynchronously.
        /// </summary>
        [PublicAPI]
        public async UniTask CloseAsync<T>(T instance = null) where T : UIWindow
        {
            await CloseInternal(instance);
        }

        /// <summary>
        ///     Force the current transition to complete.
        /// </summary>
        [PublicAPI]
        public void ForceCompleteCurrentTransition()
        {
            _sequence?.Complete(true);
        }

        #endregion


        #region Fields

        [ReadonlyInspector]
        private List<UIWindow> Stack => _stack.List;

        private readonly StackList<UIWindow> _stack = new();

        private readonly Broadcast<UIWindow> _openSequenceStarted = new();
        private readonly Broadcast<UIWindow> _openSequenceCompleted = new();
        private readonly Broadcast<UIWindow> _closeSequenceStarted = new();
        private readonly Broadcast<UIWindow> _closeSequenceCompleted = new();
        private readonly Broadcast<UIWindow, UIWindow> _transitionStarted = new();
        private readonly Broadcast<UIWindow, UIWindow> _transitionCompleted = new();
        private readonly Queue<Action> _openQueue = new();

        [Inject] [Debug] private readonly UISettings _settings;
        [Inject] [Debug] private readonly InputManager _inputManager;
        [Inject] [Debug] private readonly SelectionManager _selectionManager;

        private Sequence _sequence;

        #endregion


        #region Initialization & Shutdown

        private void Awake()
        {
            Container = gameObject.GetOrAddComponent<UIContainer>();
            UILocks.FirstAdded += LockUI;
            UILocks.LastRemoved += UnlockUI;
        }

        public void Start()
        {
            _inputManager.AddDiscreteEscapeListener(OnEscapePressed);
        }

        private void OnDestroy()
        {
            _stack.Clear();
            _inputManager.RemoveDiscreteEscapeListener(OnEscapePressed);
            UILocks.FirstAdded -= LockUI;
            UILocks.LastRemoved -= UnlockUI;
            Container.Dispose();
            Container = null;
        }

        private void OnEscapePressed()
        {
            if (_sequence.IsActive())
            {
                _sequence.Complete(true);
            }
        }

        internal void AddSceneObject(UIWindow window)
        {
            Container.Add(window.GetType(), window);
            window.State = WindowState.Open;
            var settings = window.Settings;
            if (settings.StartVisibility is false)
            {
                CloseImmediate(window);
            }
            else
            {
                _stack.PushUnique(window);
                window.GainFocus();
            }
        }

        /// <summary>
        ///     Used by Scene Windows and as a general safety to remove destroyed windows from the stack.
        /// </summary>
        internal void RemoveWindowFromStack(UIWindow window)
        {
            Container.Remove(window.GetType(), window);
        }

        #endregion


        #region Open

        private void OpenImmediateInternal<T>(T window = null) where T : UIWindow
        {
            OpenInternal(window).Forget();
            _sequence.Complete(true);
        }

        private void OpenScheduledInternal<T>(T window = null) where T : UIWindow
        {
            if (_sequence.IsActive() is false)
            {
                OpenInternal(window).Forget();
                return;
            }
            if (window is null)
            {
                Preload<T>();
            }
            _openQueue.Enqueue(() => { OpenInternal(window).Forget(); });
        }

        private async UniTask<T> OpenInternal<T>(T window = null) where T : UIWindow
        {
            window ??= await Container.Load<T>();

            // If the window is already open just return it.
            if (window.IsOpen)
            {
                return window;
            }

            // Force the current sequence to complete if opening another window was requested.
            if (_sequence.IsActive())
            {
                _sequence.Complete(true);
            }

            window.State = WindowState.Opening;
            window.OnOpenStarted();

            var isCurrentWindowActive = _stack.TryPeek(out var priorWindow);

            _transitionStarted.Raise(priorWindow, window);
            _openSequenceStarted.Raise(window);

            var settings = window.Settings;

            if (settings.OverrideNavigation)
            {
                // this could also be performed by UI in on open callbacks.
                var clearSelectionOnMouseMovement = settings.ClearSelectionOnMouseMovement;
                _selectionManager.ClearSelectionOnMouseMovement.Add(clearSelectionOnMouseMovement, window);
            }

            _sequence = DOTween.Sequence();
            _sequence.SetRecyclable(false);

            // Hide active window and remove focus.
            if (isCurrentWindowActive)
            {
                if (settings.HideUnderlyingUI)
                {
                    var hideSequence = priorWindow.HideAsync(true);
                    _sequence.Append(hideSequence);
                }

                if (priorWindow.Settings.Standalone is false)
                {
                    priorWindow.LoseFocus();
                }
            }

            // Show next window and gain focus.
            var showSequence = window.ShowAsync(false);

            // Wait for the other ui to be closed when necessary
            if (settings.WaitForOtherUIToCloseBeforeOpening)
            {
                _sequence.AppendCallback(() =>
                {
                    window.gameObject.SetActive(true);
                    _stack.PushUnique(window);
                });

                if (window.Settings.Standalone is false)
                {
                    _sequence.AppendCallback(window.GainFocus);
                }

                _sequence.Append(showSequence);
            }
            else
            {
                window.gameObject.SetActive(true);
                _stack.PushUnique(window);
                if (window.Settings.Standalone is false)
                {
                    window.GainFocus();
                }

                _sequence.Join(showSequence);
            }

            _sequence.AppendCallback(() =>
            {
                window.gameObject.SetActive(true);
                window.State = WindowState.Open;
                window.OnOpenCompleted();
                _openSequenceCompleted.Raise(window);
                _transitionCompleted.Raise(priorWindow, window);
                _sequence.SetRecyclable(true);
                _sequence = null;
            });

            // Await the transition sequence.
            await _sequence.AsyncWaitForCompletion().AsUniTask();
            return window;
        }

        #endregion


        #region Close

        private void CloseImmediateInternal<T>(T window = null) where T : UIWindow
        {
            CloseInternal(window).Forget();
            _sequence.Complete(true);
        }

        private async UniTask CloseInternal<T>(T window = null) where T : UIWindow
        {
            window ??= Container.Get<T>();

            if (window == null)
            {
                return;
            }

            // If the window is already closed just return.
            if (window.IsClosed)
            {
                return;
            }

            // Force the current sequence to complete if closing another window was requested.
            if (_sequence.IsActive())
            {
                _sequence.Complete(true);
            }

            window.State = WindowState.Closing;
            window.OnCloseStarted();
            _closeSequenceStarted.Raise(window);

            var settings = window.Settings;
            if (settings.OverrideNavigation)
            {
                // this could also be performed by UI in on open callbacks.
                _selectionManager.ClearSelectionOnMouseMovement.Remove(window);
            }

            var isActiveWindow = _stack.TryPeek(out var activeWindow) && activeWindow == window;
            _stack.Remove(window);

            var nextWindow = _stack.Peek();
            var hasNextWindow = nextWindow is not null;
            _transitionStarted.Raise(window, _stack.Peek());

            _sequence = DOTween.Sequence();
            _sequence.SetRecyclable(false);

            var hideSequence = window.HideAsync(false);
            _sequence.Append(hideSequence);
            _sequence.AppendCallback(() =>
            {
                window.OnCloseCompleted();
                window.gameObject.SetActive(false);
                window.State = WindowState.Closed;
                _closeSequenceCompleted.Raise(window);
                _transitionCompleted.Raise(window, nextWindow);
                _sequence.SetRecyclable(true);
                _sequence = null;
            });

            if (isActiveWindow)
            {
                if (window.Settings.Standalone is false)
                {
                    window.LoseFocus();
                }

                if (hasNextWindow)
                {
                    if (settings.HideUnderlyingUI)
                    {
                        nextWindow.gameObject.SetActive(true);
                        var showSequence = nextWindow.ShowAsync(true);
                        if (settings.WaitForCloseBeforeShowingPreviousUI)
                        {
                            _sequence.Append(showSequence);
                            if (nextWindow.Settings.Standalone is false)
                            {
                                _sequence.AppendCallback(nextWindow.GainFocus);
                            }
                        }
                        else
                        {
                            _sequence.Join(showSequence);
                            if (nextWindow.Settings.Standalone is false)
                            {
                                nextWindow.GainFocus();
                            }
                        }
                    }
                    else
                    {
                        if (nextWindow.Settings.Standalone is false)
                        {
                            nextWindow.GainFocus();
                        }
                    }
                }
            }

            await _sequence.AsyncWaitForCompletion().AsUniTask();
        }

        #endregion


        #region Window Handling

        private void CloseAllWindowsImmediateInternal()
        {
            Debug.Log("Loading Screen", "Close All Imm");
            while (_stack.Any())
            {
                CloseImmediate(_stack.Peek());
            }
        }

        private async UniTask CloseAllWindowsAsyncInternal()
        {
            Debug.Log("Loading Screen", "Close All");
            var buffer = new Stack<UIWindow>(_stack);
            _stack.Clear();
            while (buffer.TryPop(out var element))
            {
                try
                {
                    element.LoseFocus();
                    await CloseAsync(element);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        #endregion


        #region UI Locking

        private void LockUI()
        {
            if (_stack.TryPeek(out var window))
            {
                window.LoseFocus();
            }
        }

        private void UnlockUI()
        {
            if (_stack.TryPeek(out var window))
            {
                window.GainFocus();
            }
        }

        #endregion


        #region Queue Handling

        private void LateUpdate()
        {
            if (_sequence.IsActive())
            {
                return;
            }

            if (_openQueue.TryDequeue(out var scheduleOpenAction))
            {
                scheduleOpenAction();
            }
        }

        #endregion
    }
}