using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.UI
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIComponentToolkit : UIComponent
    {
        protected UIDocument UIDocument { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            UIDocument = GetComponent<UIDocument>();
        }

        protected sealed override void GainFocus()
        {
            if (Settings.Standalone)
            {
                return;
            }

            OnGainFocus();

            if (Settings.HideOnFocusLoss)
            {
                ShowAsync(true);
            }
            UIManager.ReturnConsumerStack.PushUnique(this);
        }

        protected sealed override void LoseFocus()
        {
            if (Settings.Standalone)
            {
                return;
            }

            UIManager.ReturnConsumerStack.Remove(this);

            if (Settings.HideOnFocusLoss)
            {
                HideAsync(true);
            }
            OnLoseFocus();
        }
    }
}