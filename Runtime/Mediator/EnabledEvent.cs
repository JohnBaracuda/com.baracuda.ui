using System;
using UnityEngine;

namespace Baracuda.UI.Mediator
{
    public class EnabledEvent : MonoBehaviour
    {
        public event Action Enabled;

        private void OnEnable()
        {
            Enabled?.Invoke();
        }
    }
}