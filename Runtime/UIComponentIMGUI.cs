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
    [RequireComponent(typeof(UIScaleController))]
    public abstract class UIComponentIMGUI : UIComponent
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
        protected override Sequence ShowAsync(bool isFocusRegain)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(1, .3f));
            return sequence;
        }

        /// <summary>
        ///     Called on the component to play custom closing or fade out effects.
        /// </summary>
        /// <param name="isFocusLoss"></param>
        protected override Sequence HideAsync(bool isFocusLoss)
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
            if (Settings.StartVisibility is false)
            {
                CanvasGroup.alpha = 0;
                IsVisible = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputManager.NavigationInputReceived -= _forceSelectObject;
            InputManager.BecameControllerScheme -= _forceSelectObject;
            InputManager.BecameDesktopScheme -= _forceDeselectObject;
            InputManager.SelectionChanged -= _cacheSelection;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            canvas ??= GetComponent<Canvas>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            buttons = GetComponentsInChildren<Button>(true);
            selectables = GetComponentsInChildren<Selectable>(true);
        }
#endif

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

        protected sealed override void GainFocus()
        {
            if (Settings.Standalone)
            {
                return;
            }
            OnGainFocus();

            if (Settings.HideOnFocusLoss)
            {
                ShowAsync(true);
            }
            UIManager.ReturnConsumerStack.PushUnique(this);

            _forceSelectObject ??= ForceSelectObject;
            _forceDeselectObject ??= ForceDeselectObject;
            _cacheSelection ??= CacheSelection;

            InputManager.NavigationInputReceived += _forceSelectObject;
            InputManager.BecameControllerScheme += _forceSelectObject;
            InputManager.BecameDesktopScheme += _forceDeselectObject;
            InputManager.SelectionChanged += _cacheSelection;

            if (InputManager.IsGamepadScheme || InputManager.InteractionMode == InteractionMode.NavigationInput)
            {
                ForceSelectObject();
            }
        }

        protected sealed override void LoseFocus()
        {
            if (Settings.Standalone)
            {
                return;
            }
            UIManager.ReturnConsumerStack.Remove(this);

            if (Settings.HideOnFocusLoss)
            {
                HideAsync(true);
            }

            InputManager.NavigationInputReceived -= _forceSelectObject;
            InputManager.BecameControllerScheme -= _forceSelectObject;
            InputManager.BecameDesktopScheme -= _forceDeselectObject;
            InputManager.SelectionChanged -= _cacheSelection;

            _lastSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            OnLoseFocus();
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

        private GameObject GetObjectToSelect()
        {
            // Check if the currently selected object is already viable.

            if (InputManager.HasSelectable && InputManager.Selected.IsActiveInHierarchy())
            {
                var selectedObject = InputManager.Selected;
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