using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/UI Component Settings")]
    public sealed class UIComponentSettings : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private bool isSceneObject;
        [SerializeField] private bool hideUnderlyingUI;
        [SerializeField] private bool hideOnFocusLoss;
        [SerializeField] private bool waitForOtherUIToCloseBeforeOpening;
        [SerializeField] private bool startVisibility = true;
        [SerializeField] private bool autoSelectFirstObject = true;
        [HideIf(nameof(autoSelectFirstObject))]
        [SerializeField] [Required] private Selectable firstSelected;
        [Tooltip("Standalone means that the UI can be handled without influencing or being influenced by other UI")]
        [SerializeField] private bool standalone;

        #endregion


        #region Properties

        public bool IsSceneObject => isSceneObject;
        public bool HideUnderlyingUI => hideUnderlyingUI;
        public bool HideOnFocusLoss => hideOnFocusLoss;
        public bool WaitForOtherUIToCloseBeforeOpening => waitForOtherUIToCloseBeforeOpening;
        public bool StartVisibility => startVisibility;
        public bool AutoSelectFirstObject => autoSelectFirstObject;
        public Selectable FirstSelected => firstSelected;
        public bool Standalone => standalone;

        #endregion
    }
}