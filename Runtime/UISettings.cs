using Baracuda.Bedrock.Settings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Baracuda.UI
{
    public class UISettings : SettingsAsset
    {
        [SerializeField] [Required] private InputActionReference returnInput;

        public InputActionReference ReturnInput => returnInput;
    }
}