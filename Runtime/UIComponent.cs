using Baracuda.Mediator.Events;
using Baracuda.Mediator.Injection;
using Baracuda.Tools;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Baracuda.UI
{
    [RequireComponent(typeof(UIComponentSettings))]
    public abstract class UIComponent : MonoBehaviour, IReturnConsumer
    {
        #region Fields

        [HideInInspector]
        [SerializeField] private UIComponentSettings uiSettings;

        #endregion


        #region Fields

        private readonly Broadcast _opened = new();
        private readonly Broadcast _closed = new();
        private readonly Broadcast _opening = new();
        private readonly Broadcast _closing = new();
        private readonly Broadcast<ViewState> _stateChanged = new();

        internal static Sequence TransitionSequence { get; set; }
        protected UIComponentSettings Settings => uiSettings;

        [Inject] [Debug]
        protected UIManager UIManager { get; }

        #endregion


        #region Properties

        public ViewState State
        {
            get => _state;
            private set => SetViewState(value);
        }

        public bool IsVisible { get; protected set; } = true;

        private void SetViewState(ViewState value)
        {
            if (_state == value)
            {
                return;
            }
            _state = value;
            _stateChanged.Raise(value);
            switch (value)
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

        private ViewState _state = ViewState.None;

        #endregion


        #region Events

        public event Action Opened
        {
            add => _opened.Add(value);
            remove => _opened.Remove(value);
        }

        public event Action Closed
        {
            add => _closed.Add(value);
            remove => _closed.Remove(value);
        }

        public event Action Opening
        {
            add => _opening.Add(value);
            remove => _opening.Remove(value);
        }

        public event Action Closing
        {
            add => _closing.Add(value);
            remove => _closing.Remove(value);
        }

        public event Action<ViewState> StateChanged
        {
            add => _stateChanged.AddUnique(value);
            remove => _stateChanged.Remove(value);
        }

        #endregion


        #region Methods

        [Line]
        [Button]
        public void Open()
        {
            OpenInternal();
        }

        [Button]
        public void OpenImmediate()
        {
            OpenImmediateInternal();
        }

        [Button]
        public UniTask OpenAsync()
        {
            return OpenAsyncInternal();
        }

        [PropertySpace]
        [Button]
        public void Close()
        {
            CloseInternal();
        }

        [Button]
        public void CloseImmediate()
        {
            CloseImmediateInternal();
        }

        [Button]
        public UniTask CloseAsync()
        {
            return CloseAsyncInternal();
        }

        #endregion


        #region Virtual Callbacks

        /// <summary>
        ///     Override this method to receive and optionally consume 'Back Pressed' or 'Return' callbacks on the UI.
        /// </summary>
        public virtual bool TryConsumeReturn()
        {
            return false;
        }

        /// <summary>
        ///     Called on the component to play custom opening or fade in effects.
        /// </summary>
        /// <param name="isFocusRegain"></param>
        protected abstract Sequence ShowAsync(bool isFocusRegain);

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        /// <param name="isFocusLoss"></param>
        protected abstract Sequence HideAsync(bool isFocusLoss);

        /// <summary>
        ///     Called when the component becomes the upper most view component.
        /// </summary>
        protected virtual void OnGainFocus()
        {
        }

        /// <summary>
        ///     Called when the component is no longer the upper most view component.
        /// </summary>
        protected virtual void OnLoseFocus()
        {
        }

        #endregion


        #region OpenAsync

        private void OpenInternal()
        {
            OpenAsyncInternal().Forget();
        }

        private async UniTask OpenAsyncInternal()
        {
            if (State is ViewState.Open)
            {
                return;
            }
            if (TransitionSequence.IsActive())
            {
                TransitionSequence.Complete(true);
            }

            State = ViewState.Opening;
            var sequence = DOTween.Sequence();

            if (UIManager.UIStack.TryPeek(out var current))
            {
                if (uiSettings.HideUnderlyingUI)
                {
                    var hideSequence = current.HideAsync(true);
                    sequence.Append(hideSequence);
                }
                current.LoseFocus();
            }

            var showSequence = ShowAsync(false);
            if (uiSettings.WaitForOtherUIToCloseBeforeOpening)
            {
                sequence.AppendCallback(Activate);
                sequence.AppendCallback(() => UIManager.UIStack.PushUnique(this));
                sequence.AppendCallback(GainFocus);
                sequence.Append(showSequence);
            }
            else
            {
                gameObject.SetActive(true);
                UIManager.UIStack.PushUnique(this);
                GainFocus();
                sequence.Join(showSequence);
            }

            sequence.AppendCallback(() => State = ViewState.Open);
            sequence.AppendCallback(Activate);

            TransitionSequence = sequence;

            await sequence.AsyncWaitForCompletion().AsUniTask();
        }

        private void OpenImmediateInternal()
        {
            OpenAsyncInternal().Forget();
            TransitionSequence.Complete(true);
        }

        #endregion


        #region Close

        private void CloseInternal()
        {
            CloseAsyncInternal().Forget();
        }

        private async UniTask CloseAsyncInternal()
        {
            if (State is ViewState.Closed)
            {
                return;
            }
            if (TransitionSequence.IsActive())
            {
                TransitionSequence.Complete(true);
            }

            State = ViewState.Closing;
            var sequence = DOTween.Sequence();
            var isActiveUI = UIManager.UIStack.TryPeek(out var current) && current == this;

            LoseFocus();

            var hideSequence = HideAsync(false);
            sequence.Append(hideSequence);
            sequence.AppendCallback(Deactivate);

            UIManager.UIStack.Remove(this);
            if (isActiveUI && UIManager.UIStack.TryPeek(out var previous))
            {
                if (uiSettings.HideUnderlyingUI)
                {
                    var showSequence = previous.ShowAsync(true);
                    previous.SetActive(true);
                    sequence.Join(showSequence);
                }
                previous.GainFocus();
            }

            sequence.AppendCallback(() => State = ViewState.Closed);

            TransitionSequence = sequence;
            await sequence.AsyncWaitForCompletion().AsUniTask();
            TransitionSequence = null;
        }

        private void CloseImmediateInternal()
        {
            CloseAsyncInternal().Forget();
            TransitionSequence.Complete(true);
        }

        #endregion


        #region Unity Callbacks

        protected virtual void Awake()
        {
            Inject.Dependencies(this);
        }

        protected virtual void Start()
        {
            if (uiSettings.IsSceneObject)
            {
                UIManager.Register(GetType(), this);
                State = ViewState.Open;
                if (uiSettings.StartVisibility is false)
                {
                    CloseImmediate();
                }
                else
                {
                    UIManager.UIStack.PushUnique(this);
                    GainFocus();
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            uiSettings ??= GetComponent<UIComponentSettings>() ?? gameObject.AddComponent<UIComponentSettings>();
        }
#endif

        protected virtual void OnDestroy()
        {
            DOTween.Kill(this);
            if (uiSettings.IsSceneObject && UIManager)
            {
                UIManager.Unregister(GetType(), this);
            }
        }

        #endregion


        #region Misc

        private void Activate()
        {
            this.SetActive(true);
        }

        private void Deactivate()
        {
            this.SetActive(false);
        }

        #endregion


        #region Focus Handling

        protected abstract void GainFocus();

        protected abstract void LoseFocus();

        #endregion
    }
}