using Baracuda.Bedrock.PlayerLoop;
using Baracuda.Bedrock.Services;
using Baracuda.UI.Selection;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    [RequireComponent(typeof(Selectable))]
    public class AutoReselection : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private bool autoSelectLastObjectOnDisable;
        [HideIf(nameof(autoSelectLastObjectOnDisable))]
        [SerializeField] [Required] private Selectable selectable;

        private bool _isSelected;

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
        }

        private void OnDisable()
        {
            if (Gameloop.IsQuitting)
            {
                return;
            }
            if (_isSelected && autoSelectLastObjectOnDisable)
            {
                _isSelected = false;
                var selectionManager = ServiceLocator.Get<SelectionManager>();
                selectionManager.Select(selectionManager.LastSelected);
            }
        }
    }
}