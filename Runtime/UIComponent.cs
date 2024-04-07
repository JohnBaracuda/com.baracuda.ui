using Baracuda.Tools;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(UIScaleController))]
    [RequireComponent(typeof(UIComponentSettings))]
    public abstract partial class UIComponent : MonoBehaviour, IReturnConsumer
    {
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


        #region Properties

        public ViewState State
        {
            get => _state;
            private set => SetViewState(value);
        }

        public bool IsVisible { get; private set; } = true;

        public Canvas Canvas => canvas;

        public UIAsset Asset { get; internal set; }

        protected CanvasGroup CanvasGroup => canvasGroup;

        protected Button[] Buttons => buttons;
        protected Selectable[] Selectables => selectables;

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
        protected virtual Sequence ShowAsync(bool isFocusRegain)
        {
            Tween.StopAll(this);
            var showSequence = Sequence.Create(Tween.Alpha(CanvasGroup, 1, .3f));
            return showSequence;
        }

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        /// <param name="isFocusLoss"></param>
        protected virtual Sequence HideAsync(bool isFocusLoss)
        {
            Tween.StopAll(this);
            var hideSequence = Sequence.Create(Tween.Alpha(CanvasGroup, 0, .3f));
            hideSequence.ChainCallback(this, self => self.SetActive(false));
            return hideSequence;
        }

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
    }
}