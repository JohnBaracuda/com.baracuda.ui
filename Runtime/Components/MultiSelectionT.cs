﻿using System;
using System.Linq;
using Baracuda.Serialization;
using Baracuda.Utility.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace Baracuda.UI.Components
{
    [RequireComponent(typeof(MultiSelection))]
    public abstract class MultiSelection<T> : MonoBehaviour where T : unmanaged, Enum
    {
        protected abstract SaveData<T> SaveData { get; }

        public MultiSelection Selection { get; private set; }

        public virtual T Value
        {
            get => SaveData.GetValue();
            set => SaveData.SetValue(value);
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

            SaveData.AddObserver(UpdateDisplayedSaveData);
        }

        private void OnDestroy()
        {
            Selection.ValueChanged -= OnValueChanged;
            SaveData.RemoveObserver(UpdateDisplayedSaveData);
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