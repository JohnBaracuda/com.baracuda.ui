using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [PublicAPI]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("UI/Tab")]
    public class Tab : MonoBehaviour
    {
        [ChildGameObjectsOnly]
        [SerializeField] [Required] private Selectable firstElement;
        [SerializeField] [Required] private CanvasGroup canvasGroup;

        public Selectable FirstElement => firstElement;

#if UNITY_EDITOR
        private void OnValidate()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
        }
#endif

        public virtual Tween FadeIn()
        {
            gameObject.SetActive(true);
            return canvasGroup.DOFade(1, .25f).SetEase(Ease.InOutSine);
        }

        public virtual Tween FadeOut()
        {
            return canvasGroup.DOFade(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => gameObject.SetActive(false));
        }
    }
}