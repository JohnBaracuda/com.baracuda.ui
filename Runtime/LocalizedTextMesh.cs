using Baracuda.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Baracuda.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMesh : MonoBehaviour
    {
        [SerializeField] private LocalizedString displayName;
        [ReadOnly]
        [SerializeField] private TMP_Text textMesh;

        private void OnEnable()
        {
            textMesh ??= GetComponent<TMP_Text>();
            displayName.StringChanged += OnLocalizedDisplayNameChanged;
        }

        private void OnDisable()
        {
            displayName.StringChanged -= OnLocalizedDisplayNameChanged;
        }

        private void OnLocalizedDisplayNameChanged(string value)
        {
            if (value.IsNotNullOrWhitespace())
            {
                textMesh.SetText(value);
            }
        }
    }
}