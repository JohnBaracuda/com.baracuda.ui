﻿using System.Collections.Generic;
using Baracuda.Bedrock.Injection;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Odin;
using Baracuda.Utilities.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public partial class SelectionManager : MonoBehaviour
    {
        [Debug] [Inject] private readonly InputManager _inputManager;

        private readonly Broadcast<Selectable> _onSelectionChanged = new();
        private readonly Broadcast _onSelectionCleared = new();

        [Debug] private GameObject _lastEventSystemSelection;

        private readonly HashSet<object> _clearSelectionOnMouseMovementProvider = new();
        private readonly HashSet<object> _clearSelectionOnMouseMovementBlocker = new();
        private readonly HashSet<object> _keepInputFieldsSelectedProvider = new();

        private void Start()
        {
            _inputManager.MouseInputReceived += OnMouseMovement;
        }

        private void OnDestroy()
        {
            _inputManager.MouseInputReceived -= OnMouseMovement;
        }

        private void LateUpdate()
        {
            UpdateEventSystemState();
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

        private void OnMouseMovement()
        {
            if (ClearSelectionOnMouseMovement && HasSelectable)
            {
                if (KeepInputFieldsSelected && Selected is TMP_InputField)
                {
                    return;
                }

                if (Selected is HoldButton { IsHoldInProgress: true })
                {
                    return;
                }

                Select(default(Selectable));
            }
        }
    }
}