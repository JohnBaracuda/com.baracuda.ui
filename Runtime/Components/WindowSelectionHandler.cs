using System;
using System.Linq;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.PlayerLoop;
using Baracuda.Bedrock.Services;
using Baracuda.Bedrock.Utilities;
using Baracuda.UI.Selection;
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
        [SerializeField] private bool autoSelectFirstGameObject = true;
        [HideIf(nameof(autoSelectFirstGameObject))]
        [SerializeField] private Selectable firstSelected;
        [SerializeField] private Selectable[] selectables;
        [SerializeField] private bool dontLoseFocusOnMouseMovement;

        private Action _forceSelectObject;
        private Action _forceDeselectObject;
        private Action<Selectable> _cacheSelection;
        private GameObject _lastSelected;

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

            var inputManager = ServiceLocator.Get<InputManager>();
            inputManager.NavigationInputReceived -= _forceSelectObject;
            inputManager.BecameControllerScheme -= _forceSelectObject;
            inputManager.BecameDesktopScheme -= _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged -= _cacheSelection;
        }

        public void OnWindowGainedFocus()
        {
            EnableSelectables();

            var inputManager = ServiceLocator.Get<InputManager>();

            _forceSelectObject ??= ForceSelectObject;
            _forceDeselectObject ??= ForceDeselectObject;
            _cacheSelection ??= CacheSelection;

            inputManager.NavigationInputReceived += _forceSelectObject;
            inputManager.BecameControllerScheme += _forceSelectObject;
            inputManager.BecameDesktopScheme += _forceDeselectObject;

            var selectionManager = ServiceLocator.Get<SelectionManager>();
            selectionManager.SelectionChanged += _cacheSelection;

            var isGamepad = inputManager.IsGamepadScheme;
            var isKeyNavigation = inputManager.InteractionMode == InteractionMode.NavigationInput;
            if (isGamepad || isKeyNavigation || forceSelectionOnGainFocus)
            {
                ForceSelectObject();
            }

            if (dontLoseFocusOnMouseMovement)
            {
                selectionManager.AddClearSelectionOnMouseMovementBlocker(this);
            }
        }

        public void OnWindowLostFocus()
        {
            DisableSelectables();

            var inputManager = ServiceLocator.Get<InputManager>();

            inputManager.NavigationInputReceived -= _forceSelectObject;
            inputManager.BecameControllerScheme -= _forceSelectObject;
            inputManager.BecameDesktopScheme -= _forceDeselectObject;

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

            // Get a predetermined first selection object.
            if (!autoSelectFirstGameObject && firstSelected)
            {
                return firstSelected.gameObject;
            }

            // Try to return the first found selectable component.
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