using Baracuda.Bedrock.Utilities;
using Baracuda.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScaleController : MonoBehaviour
    {
        [FormerlySerializedAs("canvasScaleFactorAsset")] [SerializeField] [Required]
        private SaveDataFloat canvasScaleFactor;
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
            if (canvasScaleFactor == null)
            {
                Debug.LogWarning("UI", "Canvas scale factor asset is null!", this);
                return;
            }
#endif

            canvasScaleFactor.Changed += UpdateCanvasScaleFactor;
            if (FileSystem.IsInitialized)
            {
                UpdateCanvasScaleFactor(canvasScaleFactor.Value);
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            canvasScaler ??= GetComponent<CanvasScaler>();
            if (canvasScaleFactor == null)
            {
                Debug.LogWarning("UI", $"Canvas scale factor asset of {gameObject.name} is null!", this);
                return;
            }
#endif
            canvasScaleFactor.Changed -= UpdateCanvasScaleFactor;
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            canvasScaler.scaleFactor = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}