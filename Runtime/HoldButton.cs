using System;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Services;
using Baracuda.Utilities;
using Baracuda.Utilities.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/Hold Button")]
    public class HoldButton : Selectable, IPointerClickHandler
    {
        #region Fields

        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private float holdDuration = 1f;
        [SerializeField] private bool cancelOnPointerExit = true;

        private readonly Broadcast _onHoldStarted = new();
        private readonly Broadcast _onHoldCompleted = new();
        private readonly Broadcast _onHoldCancelled = new();
        private readonly Broadcast<float> _onHoldProgress = new();

        private bool _isHolding;
        private float _holdProgress;
        private float _holdTime;

        #endregion


        #region Events

        public event Action HoldStarted
        {
            add => _onHoldStarted.Add(value);
            remove => _onHoldStarted.Remove(value);
        }

        public event Action HoldCompleted
        {
            add => _onHoldCompleted.Add(value);
            remove => _onHoldCompleted.Remove(value);
        }

        public event Action HoldCancelled
        {
            add => _onHoldCancelled.Add(value);
            remove => _onHoldCancelled.Remove(value);
        }

        public event Action<float> HoldProgress
        {
            add => _onHoldProgress.Add(value);
            remove => _onHoldProgress.Remove(value);
        }

        public bool IsHoldInProgress { get; private set; }

        #endregion


        #region Setup & Shutdown

        public void ClearCallbacks()
        {
            _onHoldStarted.Clear();
            _onHoldCompleted.Clear();
            _onHoldCancelled.Clear();
            _onHoldProgress.Clear();
        }

        #endregion


        #region Pointer Callbacks

        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactable is false)
            {
                return;
            }
            var inputManager = ServiceLocator.Get<InputManager>();
            if (inputManager.IsGamepadScheme is false)
            {
                return;
            }
            if (_isHolding)
            {
                StopHold();
            }
            else
            {
                StartHold();
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (interactable is false)
            {
                return;
            }
            StartHold();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            StopHold();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (interactable is false)
            {
                return;
            }
            if (cancelOnPointerExit)
            {
                StopHold();
            }
        }

        #endregion


        #region Hold Logic

        private void StartHold()
        {
            if (_isHolding)
            {
                return;
            }
            IsHoldInProgress = true;
            _onHoldStarted.Raise();
            _isHolding = true;
            _holdTime = 0;
        }

        private void StopHold()
        {
            if (_isHolding is false)
            {
                return;
            }
            IsHoldInProgress = false;
            _onHoldCancelled.Raise();
            _isHolding = false;
            _holdTime = 0;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            if (_isHolding is false)
            {
                return;
            }

            _holdTime += deltaTime;
            var clampedHoldTime = _holdTime.Clamp(0, holdDuration);
            var holdDelta = clampedHoldTime / holdDuration;
            _onHoldProgress.Raise(holdDelta);

            var holdCompletedThisFrame = holdDelta >= 1;
            if (holdCompletedThisFrame)
            {
                _isHolding = false;
                _holdTime = 0;
                IsHoldInProgress = false;
                _onHoldCompleted.Raise();
            }
        }

        #endregion


        #region Custom Submit Logic

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            submitAction.action.started += OnSubmitDown;
            submitAction.action.canceled += OnSubmitUp;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            StopHold();
            submitAction.action.started -= OnSubmitDown;
            submitAction.action.canceled -= OnSubmitUp;
        }

        private void OnSubmitDown(InputAction.CallbackContext context)
        {
            StartHold();
        }

        private void OnSubmitUp(InputAction.CallbackContext context)
        {
            StopHold();
        }

        #endregion
    }
}