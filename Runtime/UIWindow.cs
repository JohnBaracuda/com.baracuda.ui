using Baracuda.Bedrock.Callbacks;
using Baracuda.Bedrock.Injection;
using Baracuda.Bedrock.Services;
using DG.Tweening;
using UnityEngine;

namespace Baracuda.UI
{
    [RequireComponent(typeof(WindowSettings))]
    public abstract class UIWindow : MonoBehaviour
    {
        #region Fields

        [HideInInspector]
        [SerializeField] private WindowSettings uiSettings;

        #endregion


        #region Properties

        public bool IsOpen => State is WindowState.Open;
        public bool IsClosed => State is WindowState.Closed;

        public WindowSettings Settings => uiSettings;

        public WindowState State { get; protected internal set; }

        #endregion


        #region Abstract Methods

        /// <summary>
        ///     Override this method to receive and optionally consume 'Back Pressed' or 'Return' callbacks on the UI.
        /// </summary>
        public virtual EscapeUsage OnEscapePressed()
        {
            return EscapeUsage.IgnoredEscape;
        }

        /// <summary>
        ///     Called on the component to play custom opening or fade in effects.
        /// </summary>
        protected internal abstract Sequence ShowAsync(bool isFocusRegain);

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        protected internal abstract Sequence HideAsync(bool isFocusLoss);

        /// <summary>
        ///     Called when the ui component becomes the uppermost view component.
        /// </summary>
        protected internal abstract void GainFocus();

        /// <summary>
        ///     Called when the ui component is no longer the uppermost view component.
        /// </summary>
        protected internal abstract void LoseFocus();

        #endregion


        #region Virtual Methods

        /// <summary>
        ///     Called when the UIWindow started the opening sequence.
        /// </summary>
        protected internal virtual void OnOpenStarted()
        {
        }

        /// <summary>
        ///     Called when the UIWindow completed the opening sequence.
        /// </summary>
        protected internal virtual void OnOpenCompleted()
        {
        }

        /// <summary>
        ///     Called when the UIWindow started the closing sequence.
        /// </summary>
        protected internal virtual void OnCloseStarted()
        {
        }

        /// <summary>
        ///     Called when the UIWindow completed the closing sequence.
        /// </summary>
        protected internal virtual void OnCloseCompleted()
        {
        }

        #endregion


        #region Open & Close

        public void Close()
        {
            ServiceLocator.Get<UIManager>().Close(this);
        }

        #endregion


        #region Setup

        protected virtual void Awake()
        {
            Inject.Dependencies(this);
        }

        protected virtual void Start()
        {
            if (Settings.IsSceneObject)
            {
                var uiManager = ServiceLocator.Get<UIManager>();
                uiManager.AddSceneObject(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Gameloop.IsQuitting)
            {
                return;
            }

            this.DOKill();

            if (Settings.IsSceneObject)
            {
                var uiManager = ServiceLocator.Get<UIManager>();
                uiManager.RemoveSceneObject(this);
            }
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            uiSettings ??= GetComponent<WindowSettings>();
#endif
        }

        #endregion
    }
}