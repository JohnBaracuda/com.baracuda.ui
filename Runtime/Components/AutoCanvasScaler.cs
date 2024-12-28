using Baracuda.Serialization;
using Baracuda.Utility.Utilities;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class AutoCanvasScaler : MonoBehaviour
    {
        [SerializeField] [Required] private CanvasScaler canvasScaler;

        private const float MinScaleFactor = .1f;
        private const float MaxScaleFactor = 5f;

        [PublicAPI]
        public static SaveData<float> ScaleFactor { get; } = SaveData<float>
            .WithKey("ScaleFactor")
            .WithDefaultValue(1)
            .WithValidation(factor => factor.Clamp(0.5f, 2f))
            .WithAlias("db1286e4b1ad3074289c6c9ac885db2b")
            .WithTag("User Interface")
            .WithTag("UI");

        private void OnValidate()
        {
            canvasScaler ??= GetComponent<CanvasScaler>();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            canvasScaler ??= GetComponent<CanvasScaler>();
#endif
            ScaleFactor.ObservableValue.AddObserver(UpdateCanvasScaleFactor);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            canvasScaler ??= GetComponent<CanvasScaler>();
#endif
            ScaleFactor.ObservableValue.RemoveObserver(UpdateCanvasScaleFactor);
        }

        private void UpdateCanvasScaleFactor(float scaleFactor)
        {
            canvasScaler.scaleFactor = scaleFactor.Clamp(MinScaleFactor, MaxScaleFactor);
        }
    }
}