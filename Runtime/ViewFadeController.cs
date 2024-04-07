using Baracuda.Mediator.Values;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baracuda.UI
{
    public class ViewFadeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDurationInSeconds = .3f;
        [SerializeField] private float fadeOutDurationInSeconds = .3f;
        [SerializeField] private ValueAssetRO<bool> viewFadeEnabled;

        private bool _isPointerEnter;

        private void Awake()
        {
            viewFadeEnabled.Changed += OnViewFadeEnabledOnChanged;
            canvasGroup.alpha = viewFadeEnabled.Value ? 0 : 1;
        }

        private void OnDestroy()
        {
            viewFadeEnabled.Changed -= OnViewFadeEnabledOnChanged;
        }

        private void OnViewFadeEnabledOnChanged(bool isEnabled)
        {
            if (isEnabled is false)
            {
                FadeIn();
                return;
            }

            if (_isPointerEnter)
            {
                FadeIn();
            }
            else
            {
                FadeOut();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerEnter = true;
            if (viewFadeEnabled.Value)
            {
                FadeIn();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerEnter = false;
            if (viewFadeEnabled.Value)
            {
                FadeOut();
            }
        }

        private void FadeIn()
        {
            Tween.StopAll(this);
            Tween.Custom(this, canvasGroup.alpha, 1, fadeInDurationInSeconds,
                (controller, value) => controller.canvasGroup.alpha = value);
        }

        private void FadeOut()
        {
            Tween.StopAll(this);
            Tween.Custom(this, canvasGroup.alpha, 0, fadeOutDurationInSeconds,
                (controller, value) => controller.canvasGroup.alpha = value);
        }
    }
}