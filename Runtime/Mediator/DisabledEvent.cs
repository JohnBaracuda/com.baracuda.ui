using System;
using UnityEngine;

namespace Baracuda.UI.Mediator
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