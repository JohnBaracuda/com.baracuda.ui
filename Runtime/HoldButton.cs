using Baracuda.Mediator.Events;
using Baracuda.Utilities;
using System;
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
        private bool _holdCompleted;
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


        #region Pointer Callbacks

        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactable is false)
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
            if (_holdCompleted is false)
            {
                IsHoldInProgress = false;
                _onHoldCancelled.Raise();
            }
            _isHolding = false;
            _holdCompleted = false;
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

            var holdCompletedThisFrame = holdDelta >= 1 && _holdCompleted is false;
            if (holdCompletedThisFrame)
            {
                _holdCompleted = true;
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