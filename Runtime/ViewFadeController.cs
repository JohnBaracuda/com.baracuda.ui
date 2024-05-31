using Baracuda.Bedrock.Values;
using DG.Tweening;
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
            DOTween.Kill(this);
            var sequence = DOTween.Sequence(this);
            sequence.Append(canvasGroup.DOFade(1, fadeInDurationInSeconds));
        }

        private void FadeOut()
        {
            DOTween.Kill(this);
            var sequence = DOTween.Sequence(this);
            sequence.Append(canvasGroup.DOFade(0, fadeOutDurationInSeconds));
        }
    }
}