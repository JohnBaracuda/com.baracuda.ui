using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baracuda.UI.Mediator
{
    public class SelectionEvents : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public event Action<SelectionEvents> Selected;
        public event Action<SelectionEvents> Deselected;

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Deselected?.Invoke(this);
        }
    }
}