using Baracuda.Mediator;
using Baracuda.Serialization;
using Baracuda.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class UIScaleController : MonoBehaviour
    {
        [SerializeField] [Required] private FloatSaveAsset canvasScaleFactorAsset;
        [SerializeField] [Required] private CanvasScaler canvasScaler;

        private const float MinScaleFactor = .1f;
        private const float MaxScaleFactor = 5f;

        private void OnValidate()
        {
            canvasScaler ??= GetComponent<CanvasScaler>();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            canvasScaler ??= GetComponent<CanvasScaler>();
            if (canvasScaleFactorAsset == null)
            {
                Debug.LogWarning("UI", "Canvas scale factor asset is null!", this);
                return;
            }
#endif

            canvasScaleFactorAsset.Changed += UpdateCanvasScaleFactor;
            if (FileSystem.IsInitialized)
            {
                UpdateCanvasScaleFactor(canvasScaleFactorAsset.Value);
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            canvasScaler ??= GetComponent<CanvasScaler>();
            if (canvasScaleFactorAsset == null)
            {
                Debug.LogWarning("UI", "Canvas scale factor asset is null!", this);
                return;
            }
#endif
            canvasScaleFactorAsset.Changed -= UpdateCanvasScaleFactor;
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            canvasScaler.scaleFactor = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}