using Baracuda.UI.Selection;
using Baracuda.Utility.Services;
using UnityEngine;

namespace Baracuda.UI.Components
{
    public class WindowNavigationHandler : MonoBehaviour, IWindowFocus
    {
        [SerializeField] private bool clearSelectionOnMouseMovement;

        private SelectionManager _selectionManager;

        private void Awake()
        {
            _selectionManager = ServiceLocator.Get<SelectionManager>();
        }

        public void OnWindowGainedFocus()
        {
            if (clearSelectionOnMouseMovement)
            {
                _selectionManager.AddClearSelectionOnMouseMovementSource(this);
            }
            else
            {
                _selectionManager.AddClearSelectionOnMouseMovementBlocker(this);
            }
        }

        public void OnWindowLostFocus()
        {
            _selectionManager.RemoveClearSelectionOnMouseMovementSource(this);
            _selectionManager.RemoveClearSelectionOnMouseMovementBlocker(this);
        }
    }
}