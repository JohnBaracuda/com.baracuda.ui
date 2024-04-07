using Baracuda.Mediator.Events;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 4014
namespace Baracuda.UI
{
    public abstract partial class UIComponent
    {
        #region Fields

        [HideInInspector]
        [SerializeField] private Canvas canvas;
        [HideInInspector]
        [SerializeField] private CanvasGroup canvasGroup;
        [HideInInspector]
        [SerializeField] private Button[] buttons;
        [HideInInspector]
        [SerializeField] private Selectable[] selectables;
        [HideInInspector]
        [SerializeField] private UIComponentSettings uiSettings;

        #endregion


        #region Fields

        private readonly Broadcast _opened = new();
        private readonly Broadcast _closed = new();
        private readonly Broadcast _opening = new();
        private readonly Broadcast _closing = new();
        private readonly Broadcast<ViewState> _stateChanged = new();

        internal static Sequence TransitionSequence;

        private GameObject _lastSelected;
        private Action _forceSelectObject;
        private Action _forceDeselectObject;
        private Action<Selectable> _cacheSelection;

        private static UISystem UISystem => UISystem.Singleton;

        #endregion


        #region Properties

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


        #region Open

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
            if (State is ViewState.Opening)
            {
                TransitionSequence.Complete();
                return;
            }

            State = ViewState.Opening;
            var sequence = Sequence.Create();

            if (UISystem.UIStack.TryPeek(out var current))
            {
                if (uiSettings.HideUnderlyingUI)
                {
                    var hideSequence = current.HideAsync(true);
                    sequence.Chain(hideSequence);
                }
                current.LoseFocus();
            }

            var showSequence = ShowAsync(false);
            if (uiSettings.WaitForOtherUIToCloseBeforeOpening)
            {
                sequence.ChainCallback(this, self => self.SetActive(true));
                sequence.ChainCallback(this, self => UISystem.UIStack.PushUnique(this));
                sequence.ChainCallback(this, self => self.GainFocus());
                sequence.Chain(showSequence);
            }
            else
            {
                gameObject.SetActive(true);
                UISystem.UIStack.PushUnique(this);
                GainFocus();
                sequence.Group(showSequence);
            }

            sequence.ChainCallback(this, self => { self.State = ViewState.Open; });

            TransitionSequence = sequence;
            await sequence.ToUniTask();
        }

        private void OpenImmediateInternal()
        {
            OpenAsyncInternal().Forget();
            Debug.Log(TransitionSequence);
            TransitionSequence.Complete();
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
            if (State is ViewState.Closing)
            {
                TransitionSequence.Complete();
                return;
            }

            State = ViewState.Closing;
            var sequence = Sequence.Create();
            var isActiveUI = UISystem.UIStack.TryPeek(out var current) && current == this;

            LoseFocus();

            var hideSequence = HideAsync(false);
            sequence.Chain(hideSequence);
            sequence.ChainCallback(this, self => self.SetActive(false));

            UISystem.UIStack.Remove(this);
            if (isActiveUI && UISystem.UIStack.TryPeek(out var previous))
            {
                if (uiSettings.HideUnderlyingUI)
                {
                    var showSequence = previous.ShowAsync(true);
                    previous.SetActive(true);
                    sequence.Chain(showSequence);
                }
                previous.GainFocus();
            }

            sequence.ChainCallback(this, self => self.State = ViewState.Closed);

            TransitionSequence = sequence;
            await sequence.ToUniTask();
        }

        private void CloseImmediateInternal()
        {
            CloseAsyncInternal().Forget();
            TransitionSequence.Complete();
        }

        #endregion


        #region Unity Callbacks

        protected virtual void Awake()
        {
            if (uiSettings.StartVisibility is false)
            {
                CanvasGroup.alpha = 0;
                IsVisible = false;
            }
        }

        protected virtual void Start()
        {
            if (uiSettings.IsSceneObject)
            {
                State = ViewState.Open;
                if (uiSettings.StartVisibility is false)
                {
                    CloseImmediate();
                }
                else
                {
                    UISystem.UIStack.PushUnique(this);
                    GainFocus();
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            canvas ??= GetComponent<Canvas>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            buttons = GetComponentsInChildren<Button>(true);
            selectables = GetComponentsInChildren<Selectable>(true);
            uiSettings ??=
                GetComponent<UIComponentSettings>() ?? gameObject.AddComponent<UIComponentSettings>();
        }
#endif

        protected virtual void OnDestroy()
        {
            Controls.NavigationInputReceived -= _forceSelectObject;
            Controls.BecameControllerScheme -= _forceSelectObject;
            Controls.BecameDesktopScheme -= _forceDeselectObject;
            Controls.SelectionChanged -= _cacheSelection;
        }

        #endregion


        #region Misc

        protected void DisableSelectables()
        {
            foreach (var selectable in selectables)
            {
#if DEBUG
                if (selectable == null)
                {
                    Debug.LogWarning("UI", $"Cached selectable is null! {name}", this);
                    continue;
                }
#endif
                selectable.interactable = false;
            }
        }

        protected void EnableSelectables()
        {
            foreach (var selectable in selectables)
            {
#if DEBUG
                if (selectable == null)
                {
                    Debug.LogWarning("UI", $"Cached selectable is null! {name}", this);
                    continue;
                }
#endif
                selectable.interactable = true;
            }
        }

        #endregion


        #region Focus Handling

        private void GainFocus()
        {
            if (uiSettings.Standalone)
            {
                return;
            }
            OnGainFocus();

            if (uiSettings.HideOnFocusLoss)
            {
                ShowAsync(true);
            }
            UISystem.ReturnConsumerStack.PushUnique(this);

            _forceSelectObject ??= ForceSelectObject;
            _forceDeselectObject ??= ForceDeselectObject;
            _cacheSelection ??= CacheSelection;

            Controls.NavigationInputReceived += _forceSelectObject;
            Controls.BecameControllerScheme += _forceSelectObject;
            Controls.BecameDesktopScheme += _forceDeselectObject;
            Controls.SelectionChanged += _cacheSelection;

            if (Controls.IsGamepadScheme || Controls.InteractionMode == InteractionMode.NavigationInput)
            {
                ForceSelectObject();
            }
        }

        private void LoseFocus()
        {
            if (uiSettings.Standalone)
            {
                return;
            }
            UISystem.ReturnConsumerStack.Remove(this);

            if (uiSettings.HideOnFocusLoss)
            {
                HideAsync(true);
            }

            Controls.NavigationInputReceived -= _forceSelectObject;
            Controls.BecameControllerScheme -= _forceSelectObject;
            Controls.BecameDesktopScheme -= _forceDeselectObject;
            Controls.SelectionChanged -= _cacheSelection;

            _lastSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            OnLoseFocus();
        }

        private async void ForceSelectObject()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            var objectToSelect = GetObjectToSelect();
            if (objectToSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(objectToSelect);
            }
        }

        private void ForceDeselectObject()
        {
            _lastSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void CacheSelection(Selectable selectable)
        {
            if (selectables.Contains(selectable))
            {
                _lastSelected = selectable.gameObject;
            }
        }

        private GameObject GetObjectToSelect()
        {
            // Check if the currently selected object is already viable.

            if (Controls.HasSelected && Controls.Selected.IsActiveInHierarchy())
            {
                var selectedObject = Controls.Selected;
                if (selectedObject.interactable && Selectables.Contains(selectedObject))
                {
                    return selectedObject.gameObject;
                }
            }

            // Check if the last selected object is viable.
            var lastSelectedIsViable = _lastSelected && _lastSelected.activeInHierarchy;
            if (lastSelectedIsViable)
            {
                return _lastSelected;
            }

            // Get a predetermined first selection object.
            if (uiSettings.AutoSelectFirstObject && uiSettings.FirstSelected)
            {
                return uiSettings.FirstSelected.gameObject;
            }

            // Try to return the first found selectable component.
            var defaultSelection = Selectables.FirstOrDefault();
            return defaultSelection != null ? defaultSelection.gameObject : null;
        }

        #endregion
    }
}