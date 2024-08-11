using UnityEngine;

namespace Baracuda.UI.Components
{
    public class WindowBackgroundBlocker : MonoBehaviour, IWindowOpening, IWindowClosed
    {
        [SerializeField] private UIGroup group = UIGroup.Menu;

        public void OnWindowOpening()
        {
            UIManager.GetGroupManagerFor(group).BlockBackground(this);
        }

        public void OnWindowClosed()
        {
            UIManager.GetGroupManagerFor(group).UnblockBackground(this);
        }

        private void OnDestroy()
        {
            UIManager.GetGroupManagerFor(group).UnblockBackground(this);
        }
    }
}