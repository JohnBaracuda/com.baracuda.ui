﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baracuda.UI.Components
{
    public class SelectedEvent : MonoBehaviour, ISelectHandler
    {
        public event Action Selected;

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke();
        }
    }
}