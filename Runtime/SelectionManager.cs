using Baracuda.Bedrock.Events;
using Baracuda.Bedrock.Injection;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Odin;
using Baracuda.Utilities.Collections;
using JetBrains.Annotations;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public class SelectionManager : MonoBehaviour
    {
        #region Fields

        [Debug] [Inject] private readonly InputManager _inputManager;

        private readonly Broadcast<Selectable> _onSelectionChanged = new();
        private readonly Broadcast _onSelectionCleared = new();

        private GameObject _lastEventSystemSelection;

        #endregion


        #region Events & Properties

        public bool HasSelectable => Selected != null;
        public Selectable Selected { get; private set; }
        public Selectable LastSelected { get; private set; }
        public ValueStack<bool> ClearSelectionOnMouseMovement { get; } = new();
        public ValueStack<bool> KeepInputFieldsSelected { get; } = new();

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


        #region Unity Event Methods

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

        #endregion


        #region Public API

        [PublicAPI]
        public void Select(GameObject gameObjectToSelect)
        {
            EventSystem.current.SetSelectedGameObject(gameObjectToSelect);
        }

        [PublicAPI]
        public void Select(Selectable selectable)
        {
            Select(selectable.gameObject);
        }

        [PublicAPI]
        public bool IsSelectionContext(Component component)
        {
            if (HasSelectable is false)
            {
                return false;
            }

            return Selected.GetComponents<Component>().Any(item => item == component);
        }

        #endregion


        #region Update

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


        #region Navigation Callbacks

        private void OnMouseMovement()
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