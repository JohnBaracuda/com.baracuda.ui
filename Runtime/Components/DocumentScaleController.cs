using Baracuda.Serialization;
using Baracuda.Utility.Utilities;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.UI.Components
{
    [RequireComponent(typeof(UIDocument))]
    public class DocumentScaleController : MonoBehaviour
    {
        [SerializeField] [Required] private SaveDataFloat canvasScaleFactor;
        [SerializeField] [Required] private UIDocument uiDocument;

        private const float MinScaleFactor = .1f;
        private const float MaxScaleFactor = 5f;

        private void OnValidate()
        {
            uiDocument ??= GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            canvasScaleFactor.Changed += UpdateCanvasScaleFactor;
            UpdateCanvasScaleFactor(canvasScaleFactor.Value);
        }

        private void OnDisable()
        {
            canvasScaleFactor.Changed -= UpdateCanvasScaleFactor;
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            uiDocument.panelSettings.scale = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}