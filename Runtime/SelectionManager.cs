﻿using System;
using System.Linq;
using Baracuda.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    public partial class SelectionManager
    {
        [PublicAPI]
        public bool HasSelectable => Selected != null;

        [PublicAPI]
        public Selectable Selected { get; private set; }

        [PublicAPI]
        public Selectable LastSelected { get; private set; }

        [PublicAPI]
        public bool ClearSelectionOnMouseMovement => _clearSelectionOnMouseMovementProvider.Any() && _clearSelectionOnMouseMovementBlocker.IsEmpty();

        [PublicAPI]
        public bool KeepInputFieldsSelected => _keepInputFieldsSelectedProvider.Any();

        [PublicAPI]
        public event Action<Selectable> SelectionChanged
        {
            add => _onSelectionChanged.Add(value);
            remove => _onSelectionChanged.Remove(value);
        }

        [PublicAPI]
        public event Action SelectionCleared
        {
            add => _onSelectionCleared.Add(value);
            remove => _onSelectionCleared.Remove(value);
        }

        [PublicAPI]
        public void AddClearSelectionOnMouseMovementSource(object source)
        {
            _clearSelectionOnMouseMovementProvider.Add(source);
        }

        [PublicAPI]
        public void AddClearSelectionOnMouseMovementBlocker(object blocker)
        {
            _clearSelectionOnMouseMovementBlocker.Add(blocker);
        }

        [PublicAPI]
        public void RemoveClearSelectionOnMouseMovementSource(object source)
        {
            _clearSelectionOnMouseMovementProvider.Remove(source);
        }

        [PublicAPI]
        public void RemoveClearSelectionOnMouseMovementBlocker(object blocker)
        {
            _clearSelectionOnMouseMovementBlocker.Remove(blocker);
        }

        [PublicAPI]
        public void AddKeepInputFieldsSelectedSource(object source)
        {
            _keepInputFieldsSelectedProvider.Add(source);
        }

        [PublicAPI]
        public void RemoveKeepInputFieldsSelectedSource(object source)
        {
            _keepInputFieldsSelectedProvider.Remove(source);
        }

        [PublicAPI]
        public void Select(GameObject gameObjectToSelect)
        {
            EventSystem.current.SetSelectedGameObject(gameObjectToSelect);
        }

        [PublicAPI]
        public void Select(Selectable selectable)
        {
            Select(selectable?.gameObject);
        }

        [PublicAPI]
        public bool IsSelectionContext(Component component)
        {
            return HasSelectable && Selected.GetComponents<Component>().Any(item => item == component);
        }
    }
}