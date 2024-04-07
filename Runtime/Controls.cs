using Baracuda.Mediator.Cursor;
using Baracuda.Mediator.Events;
using Baracuda.Mediator.Singleton;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public class Controls : SingletonBehaviour<Controls>, IHideCursor
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private InputActionReference navigateInputAction;
        [SerializeField] private InputActionReference[] mouseInputActions;

        [Header("Schemes")]
        [SerializeField] private string[] controllerSchemes;
        [SerializeField] private HideCursorLocks cursorHide;

        public static bool IsGamepadScheme { get; private set; }
        public static InteractionMode InteractionMode { get; private set; }

        public static bool EnableNavigationEvents { get; set; } = true;

        public static event Action BecameControllerScheme
        {
            add => onBecameControllerScheme.Add(value);
            remove => onBecameControllerScheme.Remove(value);
        }

        public static event Action BecameDesktopScheme
        {
            add => onBecameDesktopScheme.Add(value);
            remove => onBecameDesktopScheme.Remove(value);
        }

        public static event Action NavigationInputReceived
        {
            add => onNavigationInputReceived.Add(value);
            remove => onNavigationInputReceived.Remove(value);
        }

        public static event Action MouseInputReceived
        {
            add => onMouseInputReceived.Add(value);
            remove => onMouseInputReceived.Remove(value);
        }

        public static bool HasSelected => Selected != null;
        public static Selectable Selected { get; private set; }

        public static event Action<Selectable> SelectionChanged
        {
            add => onSelectionChanged.Add(value);
            remove => onSelectionChanged.Remove(value);
        }

        public static event Action SelectionCleared
        {
            add => onSelectionCleared.Add(value);
            remove => onSelectionCleared.Remove(value);
        }

        private static readonly Broadcast onBecameControllerScheme = new();
        private static readonly Broadcast onBecameDesktopScheme = new();
        private static readonly Broadcast onNavigationInputReceived = new();
        private static readonly Broadcast onMouseInputReceived = new();

        private static readonly Broadcast<Selectable> onSelectionChanged = new();
        private static readonly Broadcast onSelectionCleared = new();

        private GameObject _lastEventSystemSelection;

        protected override void Awake()
        {
            base.Awake();

            playerInput.onControlsChanged += OnControlsChanged;
            navigateInputAction.action.performed += OnNavigationInput;
            foreach (var inputActionReference in mouseInputActions)
            {
                inputActionReference.action.performed += OnMouseInput;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            playerInput.onControlsChanged -= OnControlsChanged;
            navigateInputAction.action.performed -= OnNavigationInput;
            foreach (var inputActionReference in mouseInputActions)
            {
                inputActionReference.action.performed -= OnMouseInput;
            }
            Selected = null;
            IsGamepadScheme = false;
            onBecameControllerScheme.Clear();
            onBecameDesktopScheme.Clear();
            onNavigationInputReceived.Clear();
            onMouseInputReceived.Clear();
            onSelectionChanged.Clear();
            onSelectionCleared.Clear();
        }

        private void Update()
        {
            UpdateEventSystemState();
        }

        private void OnControlsChanged(PlayerInput input)
        {
            var wasControllerScheme = IsGamepadScheme;
            IsGamepadScheme = controllerSchemes.Contains(input.currentControlScheme);

            if (wasControllerScheme == IsGamepadScheme)
            {
                return;
            }

            if (IsGamepadScheme)
            {
                onBecameControllerScheme.Raise();
                cursorHide.Add(this);
            }
            else
            {
                onBecameDesktopScheme.Raise();
                cursorHide.Remove(this);
            }
        }

        private void OnNavigationInput(InputAction.CallbackContext context)
        {
            InteractionMode = InteractionMode.NavigationInput;
            if (EnableNavigationEvents)
            {
                onNavigationInputReceived.Raise();
            }
        }

        private void OnMouseInput(InputAction.CallbackContext context)
        {
            InteractionMode = InteractionMode.Mouse;
            //EventSystem.current.SetSelectedGameObject(null);
            if (EnableNavigationEvents)
            {
                onMouseInputReceived.Raise();
            }
        }

        private void UpdateEventSystemState()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            var selectedObject = EventSystem.current.currentSelectedGameObject;
            if (_lastEventSystemSelection == selectedObject)
            {
                return;
            }

            if (selectedObject == null)
            {
                onSelectionCleared.Raise();
                Selected = null;
                _lastEventSystemSelection = null;
                return;
            }

            Selected = selectedObject.GetComponent<Selectable>();
            if (HasSelected)
            {
                onSelectionChanged.Raise(Selected);
            }
            _lastEventSystemSelection = selectedObject;
        }
    }
}