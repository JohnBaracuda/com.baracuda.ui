﻿using System.Linq;
using Baracuda.Bedrock.Mediator;
using Baracuda.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

namespace Baracuda.UI
{
    [RequireComponent(typeof(MultiSelection))]
    public class StringSelection : MonoBehaviour
    {
        [SerializeField] private StringSelectionMode selectionMode;
        [ShowIf(nameof(selectionMode), StringSelectionMode.LocalizedTable)]
        [SerializeField] private LocalizedStringTable table;
        [ShowIf(nameof(selectionMode), StringSelectionMode.LocalizedString)]
        [SerializeField] private LocalizedString[] localizedStrings;
        [ShowIf(nameof(selectionMode), StringSelectionMode.String)]
        [SerializeField] private string[] strings;

        [Space]
        [SerializeField] private bool saveSelection;
        [ShowIf(nameof(saveSelection))]
        [SerializeField] private StringSaveAsset saveAsset;

        public MultiSelection Selection { get; private set; }

        private bool _isInitialized;
        private int? _startIndex;

        public void SelectElement(int index)
        {
            if (!_isInitialized)
            {
                _startIndex = index;
                return;
            }

            Selection.SelectElement(index);
        }

        private void Awake()
        {
            Selection = GetComponent<MultiSelection>();

            if (selectionMode is StringSelectionMode.LocalizedTable)
            {
                table?.GetTableAsync();
            }
        }

        private void Start()
        {
            var firstSelected = default(SelectionEntry);
            Selection.ValueChanged += OnValueChanged;

            if (selectionMode is StringSelectionMode.LocalizedTable)
            {
                var stringTable = table.GetTable();
                var entries = new SelectionEntry[stringTable.Count];
                var index = 0;

                foreach (var (key, value) in stringTable)
                {
                    var uniqueId = key.ToString();

                    var entry = new SelectionEntry
                    {
                        SystemName = value.LocalizedValue,
                        Index = index,
                        UniqueIdentifier = uniqueId
                    };

                    entries[index] = entry;

                    if (_startIndex == index)
                    {
                        firstSelected = entry;
                    }

                    if (saveSelection && saveAsset.Value == uniqueId)
                    {
                        firstSelected = entry;
                    }

                    index++;
                }

                if (_startIndex == -1)
                {
                    firstSelected ??= RandomUtility.GetRandomItem(entries);
                }

                firstSelected ??= entries.FirstOrDefault();

                Selection.Initialize(entries, firstSelected);
                _isInitialized = true;
            }
            else
            {
                var entries = new SelectionEntry[localizedStrings.Length];

                for (var index = 0; index < localizedStrings.Length; index++)
                {
                    var localizedString = localizedStrings[index];
                    var uniqueId = localizedString.TableEntryReference.Key;

                    var entry = new SelectionEntry
                    {
                        LocalizedName = localizedString,
                        Index = index,
                        UniqueIdentifier = uniqueId
                    };

                    entries[index] = entry;

                    if (_startIndex == index)
                    {
                        firstSelected = entry;
                    }

                    if (saveSelection && saveAsset.Value == uniqueId)
                    {
                        firstSelected = entry;
                    }
                }

                if (_startIndex == -1)
                {
                    firstSelected ??= RandomUtility.GetRandomItem(entries);
                }

                firstSelected ??= entries.FirstOrDefault();

                Selection.Initialize(entries, firstSelected);
                _isInitialized = true;
            }
        }

        private void OnDestroy()
        {
            Selection.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(SelectionEntry entry)
        {
            if (saveSelection)
            {
                saveAsset.Value = entry.UniqueIdentifier;
            }
        }
    }
}