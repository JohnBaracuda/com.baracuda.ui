using DG.Tweening;
using UnityEngine;

namespace Baracuda.UI
{
    public class HUD : UIComponentIMGUI
    {
        [SerializeField] private float fadeInDuration;
        [SerializeField] private float fadeOutDuration;

        protected override Sequence ShowAsync(bool isFocusRegain)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(1, fadeInDuration));
            return sequence;
        }

        protected override Sequence HideAsync(bool isFocusLoss)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(0, fadeOutDuration));
            return sequence;
        }

        public override bool TryConsumeReturn()
        {
            return false;
        }
    }
}