using System;
using System.Linq;
using Baracuda.Bedrock.Utilities;
using Baracuda.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace Baracuda.UI
{
    [RequireComponent(typeof(MultiSelection))]
    public class MultiSelection<T> : MonoBehaviour where T : unmanaged, Enum
    {
        [FormerlySerializedAs("value")]
        [FormerlySerializedAs("valueAsset")]
        [SerializeField] [Required] private SaveDataValueAsset<T> saveData;

        public MultiSelection Selection { get; private set; }

        public virtual T Value
        {
            get => saveData.GetValue();
            set => saveData.SetValue(value);
        }

        private bool _isInitialized;

        public void Start()
        {
            var enumValues = EnumUtility.GetValueArray<T>();
            var entries = new SelectionEntry[enumValues.Length];

            var startEntry = default(SelectionEntry);

            for (var index = 0; index < enumValues.Length; index++)
            {
                var enumValue = enumValues[index];

                var entry = new SelectionEntry
                {
                    SystemName = GetSystemName(enumValue),
                    EnumValue = EnumUtility.ToInt(enumValue),
                    LocalizedName = GetLocalizedName(enumValue),
                    Index = index
                };

                entries[index] = entry;

                if (Equals(Value, enumValue))
                {
                    startEntry = entry;
                }
            }

            startEntry ??= entries.FirstOrDefault();
            Selection.Initialize(entries, startEntry);
            Selection.ValueChanged += OnValueChanged;
            OnValueChanged(startEntry);

            saveData.Changed += UpdateDisplayedSaveData;
        }

        private void OnDestroy()
        {
            Selection.ValueChanged -= OnValueChanged;
            saveData.Changed -= UpdateDisplayedSaveData;
        }

        private void UpdateDisplayedSaveData(T value)
        {
            var entry = Selection.Entries.First(entry => entry.EnumValue == EnumUtility.ToInt(value));
            Selection.SelectElement(entry.Index);
        }

        public void SelectElement(T value)
        {
            Value = value;

            if (!_isInitialized)
            {
                return;
            }

            var entry = Selection.Entries.FirstOrDefault(entry => entry.EnumValue == EnumUtility.ToInt(value));
            Selection.SelectElement(entry!.Index);
        }

        private void OnEnable()
        {
            Selection = GetComponent<MultiSelection>();
            Selection.ValueChanged += OnValueChanged;
        }

        private void OnDisable()
        {
            Selection.ValueChanged -= OnValueChanged;
        }

        protected string GetSystemName(T value)
        {
            return value.ToString();
        }

        protected virtual LocalizedString GetLocalizedName(T value)
        {
            return null;
        }

        private void OnValueChanged(SelectionEntry selection)
        {
            var value = EnumUtility.FromInt<T>(selection.EnumValue);
            Value = value;
        }
    }
}