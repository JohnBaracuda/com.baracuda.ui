using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Baracuda.UI
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIWindowDocument : UIWindow
    {
        protected UIDocument UIDocument { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            UIDocument = GetComponent<UIDocument>();
        }

        protected internal override void GainFocus()
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            if (Settings.HideOnFocusLoss)
            {
                ShowAsync(true);
            }
            if (Settings.ListenForEscapePress)
            {
                inputManager.AddEscapeConsumer(OnEscapePressed);
            }
        }

        protected internal override void LoseFocus()
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            if (Settings.ListenForEscapePress)
            {
                inputManager.RemoveEscapeConsumer(OnEscapePressed);
            }
            if (Settings.HideOnFocusLoss)
            {
                HideAsync(true);
            }
        }
    }
}