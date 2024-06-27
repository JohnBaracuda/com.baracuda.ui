using System;
using System.Linq;
using Baracuda.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace Baracuda.UI
{
    [RequireComponent(typeof(MultiSelection))]
    public class MultiSelection<T> : MonoBehaviour where T : unmanaged, Enum
    {
        public event Action<T> ValueChanged;

        public MultiSelection Selection { get; private set; }

        public T Value { get; private set; }
        private bool _isInitialized;

        public void Start()
        {
            var enumValues = EnumUtility<T>.GetValueArray();
            var entries = new SelectionEntry[enumValues.Length];

            var startEntry = default(SelectionEntry);

            for (var index = 0; index < enumValues.Length; index++)
            {
                var value = enumValues[index];

                var entry = new SelectionEntry
                {
                    SystemName = GetSystemName(value),
                    EnumValue = EnumUtility<T>.ToInt(value),
                    LocalizedName = GetLocalizedName(value),
                    Index = index
                };

                entries[index] = entry;

                if (EnumUtility<T>.Equals(Value, value))
                {
                    startEntry = entry;
                }
            }

            startEntry ??= entries.FirstOrDefault();
            Selection.Initialize(entries, startEntry);
            Selection.ValueChanged += OnValueChanged;
            OnValueChanged(startEntry);
        }

        public void SelectElement(T value)
        {
            Value = value;

            if (!_isInitialized)
            {
                return;
            }

            var entry = Selection.Entries.FirstOrDefault(entry => entry.EnumValue == EnumUtility<T>.ToInt(value));
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

        protected virtual string GetSystemName(T value)
        {
            return value.ToString();
        }

        protected virtual LocalizedString GetLocalizedName(T value)
        {
            return null;
        }

        protected virtual bool IsValidEntry(T value)
        {
            return true;
        }

        private void OnValueChanged(SelectionEntry selection)
        {
            var value = EnumUtility<T>.FromInt(selection.EnumValue);
            Value = value;

            ValueChanged?.Invoke(value);
        }
    }
}