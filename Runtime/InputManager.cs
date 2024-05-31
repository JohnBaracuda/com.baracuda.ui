using Baracuda.Bedrock.Cursor;
using Baracuda.Bedrock.Events;
using Baracuda.Bedrock.Locks;
using Baracuda.Bedrock.Odin;
using Baracuda.Bedrock.Services;
using Baracuda.Utilities.Collections;
using Baracuda.Utilities.Types;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] [Required] private PlayerInput playerInput;
        [SerializeField] [Required] private InputActionAsset inputActionAsset;
        [SerializeField] [Required] private InputActionReference navigateInputAction;
        [SerializeField] [Required] private InputActionReference escapeInputAction;
        [SerializeField] [Required] private InputActionReference[] mouseInputActions;

        [Header("Schemes")]
        [SerializeField] private string[] controllerSchemes;

        private readonly Lock _escapeInputBlocker = new();
        private readonly StackList<Func<EscapeUsage>> _escapeConsumerStack = new();
        private readonly List<Action> _escapeListener = new();

        [ReadonlyInspector]
        private List<object> EscapeConsumer => _escapeConsumerStack.Select(item => item.Target).ToList();

        #endregion


        #region Properties

        public PlayerInput PlayerInput => playerInput;
        public bool IsGamepadScheme { get; private set; }
        public bool IsDesktopScheme => !IsGamepadScheme;
        public InteractionMode InteractionMode { get; private set; }
        public bool EnableNavigationEvents { get; set; } = true;
        public bool HasSelectable => Selected != null;
        public Selectable Selected { get; private set; }
        public Selectable LastSelected { get; private set; }

        #endregion


        #region Events

        public event Action BecameControllerScheme
        {
            add => _onBecameControllerScheme.Add(value);
            remove => _onBecameControllerScheme.Remove(value);
        }

        public event Action BecameDesktopScheme
        {
            add => _onBecameDesktopScheme.Add(value);
            remove => _onBecameDesktopScheme.Remove(value);
        }

        public event Action NavigationInputReceived
        {
            add => _onNavigationInputReceived.Add(value);
            remove => _onNavigationInputReceived.Remove(value);
        }

        public event Action MouseInputReceived
        {
            add => _onMouseInputReceived.Add(value);
            remove => _onMouseInputReceived.Remove(value);
        }

        public event Action<Selectable> SelectionChanged
        {
            add => _onSelectionChanged.Add(value);
            remove => _onSelectionChanged.Remove(value);
        }

        public event Action SelectionCleared
        {
            add => _onSelectionCleared.Add(value);
            remove => _onSelectionCleared.Remove(value);
        }

        #endregion


        #region Fields

        private readonly Broadcast _onBecameControllerScheme = new();
        private readonly Broadcast _onBecameDesktopScheme = new();
        private readonly Broadcast _onNavigationInputReceived = new();
        private readonly Broadcast _onMouseInputReceived = new();

        private readonly Broadcast<Selectable> _onSelectionChanged = new();
        private readonly Broadcast _onSelectionCleared = new();

        private GameObject _lastEventSystemSelection;

        [ReadonlyInspector]
        private readonly Dictionary<InputActionMapReference, HashSet<object>> _inputActionMapProvider = new();

        #endregion


        #region Input Map Provider

        public void EnableActionMap(InputActionMapReference actionMapReference, object provider)
        {
            if (!_inputActionMapProvider.TryGetValue(actionMapReference, out var hashSet))
            {
                hashSet = new HashSet<object>();
                _inputActionMapProvider.Add(actionMapReference, hashSet);
            }

            hashSet.Add(provider);
        }

        public void DisableActionMap(InputActionMapReference actionMapReference, object provider)
        {
            if (!_inputActionMapProvider.TryGetValue(actionMapReference, out var hashSet))
            {
                hashSet = new HashSet<object>();
                _inputActionMapProvider.Add(actionMapReference, hashSet);
            }

            hashSet.Remove(provider);
        }

        private void UpdateInputActionMaps()
        {
            inputActionAsset.Enable();

            foreach (var (inputActionMapName, hashSet) in _inputActionMapProvider.Reverse())
            {
                var actionMap = inputActionAsset.FindActionMap(inputActionMapName, true);
                var desiredState = hashSet.Any();

                if (desiredState == actionMap.enabled)
                {
                    continue;
                }

                if (desiredState)
                {
                    actionMap.Enable();
                }
                else
                {
                    actionMap.Disable();
                }
            }
        }

        #endregion


        #region EscapeUsage Listener

        public void AddEscapeConsumer(Func<EscapeUsage> listener)
        {
            _escapeConsumerStack.PushUnique(listener);
        }

        public void RemoveEscapeConsumer(Func<EscapeUsage> listener)
        {
            _escapeConsumerStack.Remove(listener);
        }

        public void AddDiscreteEscapeListener(Action listener)
        {
            _escapeListener.Add(listener);
        }

        public void RemoveDiscreteEscapeListener(Action listener)
        {
            _escapeListener.Remove(listener);
        }

        public void BlockEscapeInput(object blocker)
        {
            _escapeInputBlocker.Add(blocker);
        }

        public async void UnlockEscapeInput(object blocker)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            _escapeInputBlocker.Remove(blocker);
        }

        private void OnEscapePressed(InputAction.CallbackContext context)
        {
            if (_escapeInputBlocker.HasAny())
            {
                return;
            }

            foreach (var action in _escapeListener)
            {
                action();
            }

            foreach (var consumer in _escapeConsumerStack.Reverse())
            {
                if (consumer() is EscapeUsage.ConsumedEscape)
                {
                    break;
                }
            }
        }

        #endregion


        #region Setup & Shutdown

        private void Awake()
        {
            playerInput.onControlsChanged += OnControlsChanged;
            navigateInputAction.action.performed += OnNavigationInput;
            inputActionAsset.Enable();
            foreach (var inputActionMap in inputActionAsset.actionMaps)
            {
                inputActionMap.Disable();
            }
            foreach (var inputActionReference in mouseInputActions)
            {
                inputActionReference.action.performed += OnMouseInput;
            }
            escapeInputAction.action.performed += OnEscapePressed;
        }

        private void Start()
        {
            playerInput.uiInputModule = ServiceLocator.Get<InputSystemUIInputModule>();
        }

        private void OnDestroy()
        {
            playerInput.onControlsChanged -= OnControlsChanged;
            navigateInputAction.action.performed -= OnNavigationInput;
            foreach (var inputActionReference in mouseInputActions)
            {
                inputActionReference.action.performed -= OnMouseInput;
            }
            escapeInputAction.action.performed -= OnEscapePressed;
            LastSelected = Selected is SelectionRouter ? LastSelected : Selected;
            Selected = null;
            IsGamepadScheme = false;
            _onBecameControllerScheme.Clear();
            _onBecameDesktopScheme.Clear();
            _onNavigationInputReceived.Clear();
            _onMouseInputReceived.Clear();
            _onSelectionChanged.Clear();
            _onSelectionCleared.Clear();
        }

        private void LateUpdate()
        {
            UpdateEventSystemState();
            UpdateInputActionMaps();
        }

        #endregion


        #region Selection

        public void Select(GameObject gameObjectToSelect)
        {
            EventSystem.current.SetSelectedGameObject(gameObjectToSelect);
        }

        public void Select(Selectable selectable)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        public bool IsSelectionContext(Component component)
        {
            if (HasSelectable is false)
            {
                return false;
            }

            return Selected.GetComponents<Component>().Any(item => item == component);
        }

        private void OnControlsChanged(PlayerInput input)
        {
            var wasControllerScheme = IsGamepadScheme;
            IsGamepadScheme = controllerSchemes.Contains(input.currentControlScheme);

            if (wasControllerScheme == IsGamepadScheme)
            {
                return;
            }

            var cursorManager = ServiceLocator.Get<CursorManager>();
            if (IsGamepadScheme)
            {
                _onBecameControllerScheme.Raise();
                cursorManager.AddCursorVisibilityBlocker(this);
            }
            else
            {
                _onBecameDesktopScheme.Raise();
                cursorManager.RemoveCursorVisibilityBlocker(this);
            }
        }

        private void OnNavigationInput(InputAction.CallbackContext context)
        {
            InteractionMode = InteractionMode.NavigationInput;
            if (EnableNavigationEvents)
            {
                _onNavigationInputReceived.Raise();
            }
        }

        private void OnMouseInput(InputAction.CallbackContext context)
        {
            InteractionMode = InteractionMode.Mouse;
            if (EnableNavigationEvents)
            {
                _onMouseInputReceived.Raise();
                OnMouseMovementInternal();
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
                _onSelectionCleared.Raise();
                LastSelected = Selected is SelectionRouter ? LastSelected : Selected;
                Selected = null;
                _lastEventSystemSelection = null;
                return;
            }

            LastSelected = Selected is SelectionRouter ? LastSelected : Selected;
            Selected = selectedObject.GetComponent<Selectable>();
            if (HasSelectable)
            {
                _onSelectionChanged.Raise(Selected);
            }
            _lastEventSystemSelection = selectedObject;
        }

        #endregion


        #region Navigation

        public ValueStack<bool> ClearSelectionOnMouseMovement { get; } = new();
        public ValueStack<bool> KeepInputFieldsSelected { get; } = new();

        private void OnMouseMovementInternal()
        {
            if (ClearSelectionOnMouseMovement && HasSelectable)
            {
                if (KeepInputFieldsSelected.Value && Selected is TMP_InputField)
                {
                    return;
                }
                if (Selected is HoldButton {IsHoldInProgress: true})
                {
                    return;
                }
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        #endregion
    }
}