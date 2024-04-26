using Baracuda.Mediator.Settings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Baracuda.UI
{
    public class CanvasSystemSettings : SettingsAsset
    {
        [SerializeField] [Required] private InputActionReference returnInput;

        public InputActionReference ReturnInput => returnInput;
    }
}