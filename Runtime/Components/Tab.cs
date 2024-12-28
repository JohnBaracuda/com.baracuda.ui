using DG.Tweening;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.UI.Components
{
    [PublicAPI]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("UI/Tab")]
    public class Tab : MonoBehaviour
    {
        [ReadOnly]
        [SerializeField] private CanvasGroup canvasGroup;

#if UNITY_EDITOR
        private void OnValidate()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
        }
#endif

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
        }

        public virtual Tween FadeIn()
        {
            gameObject.SetActive(true);
            return canvasGroup.DOFade(1, .25f).SetEase(Ease.InOutSine);
        }

        public virtual Tween FadeOut()
        {
            Assert.IsNotNull(canvasGroup);
            Assert.IsNotNull(gameObject);
            return canvasGroup.DOFade(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => gameObject.SetActive(false));
        }
    }
}