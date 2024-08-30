using Baracuda.Bedrock.Utilities;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Baracuda.UI.Localization
{
    [ExecuteAlways]
    public class LocalizedTextMesh : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;
        [ReadOnly] [SerializeField] private TMP_Text textMesh;

        private void OnValidate()
        {
            textMesh ??= GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            textMesh ??= GetComponentInChildren<TMP_Text>(true);
            localizedString.StringChanged += OnLocalizedLocalizedStringChanged;
        }

        private void OnDisable()
        {
            localizedString.StringChanged -= OnLocalizedLocalizedStringChanged;
        }

        private void OnLocalizedLocalizedStringChanged(string value)
        {
            if (value.IsNotNullOrWhitespace())
            {
                textMesh.SetText(value);
            }
        }
    }
}