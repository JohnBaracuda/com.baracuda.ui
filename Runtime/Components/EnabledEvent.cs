using System;
using UnityEngine;

namespace Baracuda.UI.Components
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