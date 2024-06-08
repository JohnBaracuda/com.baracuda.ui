using Baracuda.Bedrock.Mediator;
using Baracuda.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DocumentScaleController : MonoBehaviour
    {
        [SerializeField] [Required] private FloatSaveAsset canvasScaleFactorAsset;
        [SerializeField] [Required] private UIDocument uiDocument;

        private const float MinScaleFactor = .1f;
        private const float MaxScaleFactor = 5f;

        private void OnValidate()
        {
            uiDocument ??= GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            canvasScaleFactorAsset.Changed += UpdateCanvasScaleFactor;
            UpdateCanvasScaleFactor(canvasScaleFactorAsset.Value);
        }

        private void OnDisable()
        {
            canvasScaleFactorAsset.Changed -= UpdateCanvasScaleFactor;
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            uiDocument.panelSettings.scale = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}