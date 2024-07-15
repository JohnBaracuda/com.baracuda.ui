using UnityEngine;

namespace Baracuda.UI.Components
{
    public class WindowTransitionSettingsHandler : MonoBehaviour, IWindowTransitionSettings
    {
        [SerializeField] private TransitionSettings transitionSettings;
        public TransitionSettings TransitionSettings => transitionSettings;
    }
}