using Baracuda.Bedrock.Assets;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Baracuda.UI
{
    public class UISettings : ScriptableAsset
    {
        [SerializeField] [Required] private InputActionReference returnInput;

        public InputActionReference ReturnInput => returnInput;
    }
}