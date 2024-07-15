using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/Window Settings")]
    public sealed class WindowSettings : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private bool listenForEscapePress = true;
        [SerializeField] private bool isSceneObject;
        [SerializeField] private bool hideUnderlyingUI;
        [SerializeField] private bool hideOnFocusLoss;
        [SerializeField] private bool waitForOtherUIToCloseBeforeOpening;
        [SerializeField] private bool waitForCloseBeforeShowingPreviousUI;
        [SerializeField] private bool startVisibility = true;
        [SerializeField] private bool autoSelectFirstObject = true;
        [SerializeField] private bool forceFirstObjectSelection;
        [HideIf(nameof(autoSelectFirstObject))]
        [SerializeField] [Required] private Selectable firstSelected;
        [SerializeField] private bool standalone;
        [InfoBox("Use WindowNavigationHandler")]
        [SerializeField] private bool overrideNavigation;
        [ShowIf(nameof(overrideNavigation))]
        [InfoBox("Use WindowNavigationHandler")]
        [SerializeField] private bool clearSelectionOnMouseMovement;
        [SerializeField] private bool manageSortingOrder = true;

        #endregion


        #region Properties

        public bool ListenForEscapePress => listenForEscapePress;
        public bool IsSceneObject => isSceneObject;
        public bool HideUnderlyingUI => hideUnderlyingUI;
        public bool HideOnFocusLoss => hideOnFocusLoss;
        public bool WaitForOtherUIToCloseBeforeOpening => waitForOtherUIToCloseBeforeOpening;
        public bool WaitForCloseBeforeShowingPreviousUI => waitForCloseBeforeShowingPreviousUI;
        public bool StartVisibility => startVisibility;
        public bool AutoSelectFirstObject => autoSelectFirstObject;
        public bool ForceFirstObjectSelection => forceFirstObjectSelection;
        public Selectable FirstSelected => firstSelected;
        public bool Standalone => standalone;
        public bool OverrideNavigation => overrideNavigation;
        public bool ClearSelectionOnMouseMovement => clearSelectionOnMouseMovement;
        public bool ManageSortingOrder => manageSortingOrder;

        #endregion
    }
}