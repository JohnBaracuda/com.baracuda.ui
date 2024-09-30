using Baracuda.Utility.Input;
using Baracuda.Utility.Services;
using UnityEngine;

namespace Baracuda.UI.Components
{
    public class WindowInputHandler : MonoBehaviour, IWindowFocus, IWindowOpening, IWindowClosing
    {
        [SerializeField] private InputActionMapReference[] focusInputs;
        [SerializeField] private InputActionMapReference[] openInputs;

        private InputManager _inputManager;

        private void Awake()
        {
            _inputManager = ServiceLocator.Get<InputManager>();
        }

        public void OnWindowGainedFocus()
        {
            foreach (var inputActionMapReference in focusInputs)
            {
                _inputManager.AddActionMapSource(inputActionMapReference, this);
            }
        }

        public void OnWindowLostFocus()
        {
            foreach (var inputActionMapReference in focusInputs)
            {
                _inputManager.RemoveActionMapSource(inputActionMapReference, this);
            }
        }

        public void OnWindowOpening()
        {
            foreach (var inputActionMapReference in openInputs)
            {
                _inputManager.AddActionMapSource(inputActionMapReference, this);
            }
        }

        public void OnWindowClosing()
        {
            foreach (var inputActionMapReference in openInputs)
            {
                _inputManager.RemoveActionMapSource(inputActionMapReference, this);
            }
        }
    }
}