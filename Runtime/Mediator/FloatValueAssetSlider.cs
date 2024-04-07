using Baracuda.Mediator.Values;
using Baracuda.Utilities;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Baracuda.UI.Mediator
{
    [ExecuteAlways]
    public class FloatValueAssetSlider : MonoBehaviour
    {
        [SerializeField] private LocalizedString displayName;
        [SerializeField] private LocalizedString description;
        [Space]
        [SerializeField] private int minValue;
        [SerializeField] private int maxValue = 1;
        [SerializeField] private float conversionFactor = 1;
        [Space]
        [SerializeField] private ValueAssetRW<float> valueAsset;
        [Space]
        [SerializeField] private Slider slider;
        [SerializeField] private SelectedEvent selectedEvent;
        [SerializeField] private TMP_Text descriptionTextField;
        [SerializeField] private TMP_Text nameTextField;
        [SerializeField] private TMP_Text valueTextField;

        private void OnEnable()
        {
            slider.wholeNumbers = true;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = valueAsset.Value / conversionFactor;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            valueTextField.text = (valueAsset.Value / conversionFactor).ToString(CultureInfo.InvariantCulture);
            displayName.StringChanged += OnLocalizedDisplayNameChanged;
            selectedEvent.Selected += UpdateDescription;
        }

        private void UpdateDescription()
        {
            descriptionTextField.text = description.GetLocalizedString();
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            displayName.StringChanged -= OnLocalizedDisplayNameChanged;
            selectedEvent.Selected -= UpdateDescription;
        }

        private void OnSliderValueChanged(float sliderValue)
        {
#if UNITY_EDITOR
            if (Application.isPlaying is false)
            {
                return;
            }
#endif
            valueAsset.Value = sliderValue * conversionFactor;
            valueTextField.text = (valueAsset.Value / conversionFactor).ToString(CultureInfo.InvariantCulture);
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