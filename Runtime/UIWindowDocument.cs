using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Services;
using DG.Tweening;
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
            Build(UIDocument.rootVisualElement, UIDocument);
        }

        protected abstract void Build(VisualElement root, UIDocument document);

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

        protected internal override Sequence ShowAsync(bool isFocusRegain)
        {
            gameObject.SetActive(true);
            return DOTween.Sequence();
        }

        protected internal override Sequence HideAsync(bool isFocusLoss)
        {
            gameObject.SetActive(false);
            return DOTween.Sequence();
        }
    }
}