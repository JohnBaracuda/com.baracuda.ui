using Baracuda.Utility.Utilities;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.UI.Components
{
    [RequireComponent(typeof(UIDocument))]
    public class DocumentScaleController : MonoBehaviour
    {
        [SerializeField] [Required] private UIDocument uiDocument;

        private const float MinScaleFactor = .1f;
        private const float MaxScaleFactor = 5f;

        private void OnValidate()
        {
            uiDocument ??= GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            AutoCanvasScaler.ScaleFactor.AddObserver(UpdateCanvasScaleFactor);
        }

        private void OnDisable()
        {
            AutoCanvasScaler.ScaleFactor.RemoveObserver(UpdateCanvasScaleFactor);
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            uiDocument.panelSettings.scale = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}