using Baracuda.Mediator.Values;
using Baracuda.Serialization;
using Baracuda.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Baracuda.UI.Mediator
{
    [ExecuteAlways]
    public class BoolValueAssetToggle : MonoBehaviour
    {
        [SerializeField] private LocalizedString displayName;
        [Space]
        [SerializeField] private ValueAssetRW<bool> valueAsset;
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text nameTextField;

        private void OnEnable()
        {
            toggle.isOn = valueAsset.Value;
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            displayName.StringChanged += OnLocalizedDisplayNameChanged;
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            displayName.StringChanged -= OnLocalizedDisplayNameChanged;
        }

        private void OnToggleValueChanged(bool value)
        {
#if UNITY_EDITOR
            if (Application.isPlaying is false)
            {
                return;
            }
#endif
            valueAsset.Value = value;
        }

        private void OnLocalizedDisplayNameChanged(string value)
        {
            if (value.IsNotNullOrWhitespace())
            {
                nameTextField.SetText(value);
            }
        }
    }
}