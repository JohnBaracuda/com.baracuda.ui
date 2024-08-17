using System;
using UnityEngine;

namespace Baracuda.UI.Components
{
    public class DisabledEvent : MonoBehaviour
    {
        public event Action Disabled;

        private void OnDisable()
        {
            Disabled?.Invoke();
        }
    }
}