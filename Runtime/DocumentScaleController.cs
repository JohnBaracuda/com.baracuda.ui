using Baracuda.Bedrock.Utilities;
using Baracuda.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Baracuda.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DocumentScaleController : MonoBehaviour
    {
        [FormerlySerializedAs("canvasScaleFactorAsset")] [SerializeField] [Required]
        private SaveDataFloat canvasScaleFactor;
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