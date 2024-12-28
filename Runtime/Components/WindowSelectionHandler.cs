using System;
using System.Linq;
using Baracuda.UI.Selection;
using Baracuda.Utility.Input;
using Baracuda.Utility.PlayerLoop;
using Baracuda.Utility.Services;
using Baracuda.Utility.Utilities;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    public class WindowSelectionHandler : MonoBehaviour, IWindowFocus
    {
        [SerializeField] private bool forceSelectionOnGainFocus;
        [SerializeField] private bool rememberSelection = true;
        [SerializeField] private bool autoSelectFirstGameObject = true;
        [HideIf(nameof(autoSelectFirstGameObject))]
        [SerializeField] private bool alwaysUseFirstSelected = false;
        [HideIf(nameof(autoSelectFirstGameObject))]
        [SerializeField] private Selectable firstSelected;
        [SerializeField] private Selectable[] selectables;
        [SerializeField] private bool dontLoseFocusOnMouseMovement;

        private Action _forceSelectObject;
        private Action _forceDeselectObject;
        private Action<Selectable> _cacheSelection;
        private GameObject _lastSelected;
        private Selectable _selectableOnLostFocus;

        private InputManager _inputManager;
        private SelectionManager _selectionManager;

        private void Awake()
        {
            ServiceLocator.Inject(ref _inputManager);
            ServiceLocator.Inject(ref _selectionManager);
        }

        public void Configure(Selectable firstSelectable, Selectable[] allSelectables)
        {
            firstSelected = firstSelectable;
            selectables = allSelectables;
        }

        private void OnValidate()
        {
            selectables = GetComponentsInChildren<Selectable>(true);
        }

        private void OnDestroy()
        {
            if (Gameloop.IsQuitting)
            {
                return;
            }

            _inputManager.NavigationInputReceived -= _forceSelectObject;
            _inputManager.BecameControllerScheme -= _forceSelectObject;
            _inputManager.BecameDesktopScheme -= _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged -= _cacheSelection;
        }

        public void OnWindowGainedFocus()
        {
            EnableSelectables();

            _forceSelectObject ??= ForceSelectObject;
            _forceDeselectObject ??= ForceDeselectObject;
            _cacheSelection ??= CacheSelection;

            _inputManager.NavigationInputReceived += _forceSelectObject;
            _inputManager.BecameControllerScheme += _forceSelectObject;
            _inputManager.BecameDesktopScheme += _forceDeselectObject;

            _selectionManager.SelectionChanged += _cacheSelection;

            var isGamepad = _inputManager.IsGamepadScheme;
            var isKeyNavigation = _inputManager.InteractionMode == InteractionMode.NavigationInput;
            if (isGamepad || isKeyNavigation || forceSelectionOnGainFocus)
            {
                ForceSelectObject();
            }

            if (dontLoseFocusOnMouseMovement)
            {
                _selectionManager.AddClearSelectionOnMouseMovementBlocker(this);
            }
        }

        public void OnWindowLostFocus()
        {
            _selectableOnLostFocus = _selectionManager.Selected;

            DisableSelectables();

            _inputManager.NavigationInputReceived -= _forceSelectObject;
            _inputManager.BecameControllerScheme -= _forceSelectObject;
            _inputManager.BecameDesktopScheme -= _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged -= _cacheSelection;
            selectionManager.RemoveClearSelectionOnMouseMovementBlocker(this);

            EventSystem.current.SetSelectedGameObject(null);
        }

        private void ForceSelectObject()
        {
            ForceSelectObjectWithDelay().Forget();
        }

        private async UniTaskVoid ForceSelectObjectWithDelay()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (EventSystem.current.currentSelectedGameObject.IsActiveAnEnabled())
            {
                return;
            }

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
            if (rememberSelection && _selectableOnLostFocus.IsActiveInHierarchy())
            {
                return _selectableOnLostFocus.gameObject;
            }

            if (!autoSelectFirstGameObject && alwaysUseFirstSelected)
            {
                return firstSelected.gameObject;
            }

            // Check if the currently selected object is already viable.
            if (_selectionManager.HasSelectable && _selectionManager.Selected.IsActiveInHierarchy())
            {
                var selectedObject = _selectionManager.Selected;

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

            if (!autoSelectFirstGameObject && firstSelected)
            {
                return firstSelected.gameObject;
            }

            foreach (var selectable in selectables)
            {
                if (selectable.IsActiveInHierarchy())
                {
                    return selectable.gameObject;
                }
            }
            return firstSelected?.gameObject;
        }

        public void DisableSelectables()
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

        public void EnableSelectables()
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
    }
}