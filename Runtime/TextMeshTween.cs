using DG.Tweening;
using TMPro;

namespace Baracuda.UI
{
    public static class DOTweenExtensions
    {
        public static Tweener DOTextMeshProSpacing(this TMP_Text target, float endValue, float duration)
        {
            return DOTween.To(() => target.characterSpacing, x => target.characterSpacing = x, endValue, duration);
        }
    }
}