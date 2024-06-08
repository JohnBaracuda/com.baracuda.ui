using Baracuda.Bedrock.Injection;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.PlayerLoop;
using Baracuda.Bedrock.Services;
using Baracuda.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CanvasScaleController))]
    public abstract class UIWindowCanvas : UIWindow
    {
        [HideInInspector]
        [SerializeField] private Canvas canvas;
        [HideInInspector]
        [SerializeField] private CanvasGroup canvasGroup;
        [HideInInspector]
        [SerializeField] private Button[] buttons;
        [HideInInspector]
        [SerializeField] private Selectable[] selectables;

        public Canvas Canvas => canvas;
        protected CanvasGroup CanvasGroup => canvasGroup;

        protected Button[] Buttons => buttons;
        protected Selectable[] Selectables => selectables;

        private Action _forceSelectObject;
        private Action _forceDeselectObject;
        private Action<Selectable> _cacheSelection;
        private GameObject _lastSelected;

        /// <summary>
        ///     Called on the component to play custom opening or fade in effects.
        /// </summary>
        /// <param name="isFocusRegain"></param>
        protected internal override Sequence ShowAsync(bool isFocusRegain)
        {
            this.DOKill();
            this.SetActive(true);
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(1, .3f));
            return sequence;
        }

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        /// <param name="isFocusLoss"></param>
        protected internal override Sequence HideAsync(bool isFocusLoss)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(0, .3f));
            sequence.AppendCallback(() => this.SetActive(false));
            return sequence;
        }

        protected override void Awake()
        {
            base.Awake();
            Inject.Dependencies(this, false);
            if (Settings.StartVisibility is false)
            {
                CanvasGroup.alpha = 0;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Gameloop.IsQuitting)
            {
                return;
            }
            var inputManager = ServiceLocator.Get<InputManager>();
            inputManager.NavigationInputReceived -= _forceSelectObject;
            inputManager.BecameControllerScheme -= _forceSelectObject;
            inputManager.BecameDesktopScheme -= _forceDeselectObject;
            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged -= _cacheSelection;
        }

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();
            canvas ??= GetComponent<Canvas>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            buttons = GetComponentsInChildren<Button>(true);
            selectables = GetComponentsInChildren<Selectable>(true);
#endif
        }

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

        protected internal override void GainFocus()
        {
            var inputManager = ServiceLocator.Get<InputManager>();

            if (Settings.HideOnFocusLoss)
            {
                ShowAsync(true);
            }
            if (Settings.ListenForEscapePress)
            {
                inputManager.AddEscapeConsumer(OnEscapePressed);
            }

            _forceSelectObject ??= ForceSelectObject;
            _forceDeselectObject ??= ForceDeselectObject;
            _cacheSelection ??= CacheSelection;

            EnableSelectables();
            inputManager.NavigationInputReceived += _forceSelectObject;
            inputManager.BecameControllerScheme += _forceSelectObject;
            inputManager.BecameDesktopScheme += _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged += _cacheSelection;

            if (inputManager.IsGamepadScheme ||
                inputManager.InteractionMode == InteractionMode.NavigationInput ||
                Settings.ForceFirstObjectSelection)
            {
                ForceSelectObject();
            }
        }

        protected internal override void LoseFocus()
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            if (Settings.ListenForEscapePress)
            {
                inputManager.RemoveEscapeConsumer(OnEscapePressed);
            }

            if (Settings.HideOnFocusLoss)
            {
                HideAsync(true);
            }

            DisableSelectables();

            inputManager.NavigationInputReceived -= _forceSelectObject;
            inputManager.BecameControllerScheme -= _forceSelectObject;
            inputManager.BecameDesktopScheme -= _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged -= _cacheSelection;

            EventSystem.current.SetSelectedGameObject(null);
        }

        private void ForceSelectObject()
        {
            ForceSelectObjectWithDelay().Forget();
        }

        private async UniTaskVoid ForceSelectObjectWithDelay()
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

        public GameObject GetObjectToSelect(bool ignoreActiveState = false)
        {
            var selectionManager = ServiceLocator.Get<SelectionManager>();
            // Check if the currently selected object is already viable.
            if (selectionManager.HasSelectable && selectionManager.Selected.IsActiveInHierarchy())
            {
                var selectedObject = selectionManager.Selected;
                if (selectedObject.interactable)
                {
                    return selectedObject.gameObject;
                }
            }

            // Check if the last selected object is viable.
            var lastSelectedIsViable = _lastSelected != null && (ignoreActiveState || _lastSelected.activeInHierarchy);
            if (lastSelectedIsViable)
            {
                return _lastSelected;
            }

            // GetIfLoaded a predetermined first selection object.
            if (Settings.AutoSelectFirstObject && Settings.FirstSelected)
            {
                return Settings.FirstSelected.gameObject;
            }

            // Try to return the first found selectable component.
            var defaultSelection = Selectables.FirstOrDefault();
            return defaultSelection != null ? defaultSelection.gameObject : null;
        }
    }
}