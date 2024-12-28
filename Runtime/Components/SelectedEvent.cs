using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    public class SelectedEvent : MonoBehaviour, ISelectHandler
    {
        public event Action Selected;
        public event Action<Selectable> SelectedSelf;

        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke();
            SelectedSelf?.Invoke(_selectable);
        }
    }
}