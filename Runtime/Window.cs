using Baracuda.UI.Components;
using Baracuda.Utility.Utilities;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(AutoCanvasScaler))]
    public abstract class Window : MonoBehaviour, IWindow
    {
        [SerializeField] private UIGroup group = UIGroup.Menu;
        [SerializeField] [Required] private Canvas canvas;
        [SerializeField] [Required] private CanvasGroup canvasGroup;
        [SerializeField] private float defaultFadeInDuration = .3f;
        [SerializeField] private float defaultFadeOutDuration = .3f;
        [SerializeField] private TransitionSettings transitionSettings = TransitionSettings.None;

        public TransitionSettings GetTransitionSettings()
        {
            return transitionSettings;
        }

        public Canvas Canvas => canvas;
        protected CanvasGroup CanvasGroup => canvasGroup;

        protected virtual void Awake()
        {
            CanvasGroup.alpha = 0;
        }

        protected virtual void OnDestroy()
        {
            this.DOKill();
        }

        protected virtual void OnValidate()
        {
            canvas ??= GetComponent<Canvas>();
            canvasGroup ??= GetComponent<CanvasGroup>();
        }

        public virtual Sequence ShowAsync(UIContext context)
        {
            this.DOKill();
            this.SetActive(true);
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(1, defaultFadeInDuration));
            return sequence;
        }

        public virtual Sequence HideAsync(UIContext context)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(0, defaultFadeOutDuration));
            sequence.AppendCallback(() =>
            {
                Assert.IsNotNull(this);
                this.SetActive(false);
            });

            return sequence;
        }

        public virtual UIGroup GetDefaultGroup()
        {
            return group;
        }

        public virtual void SetSortingOrder(int sortingOrder)
        {
            Canvas.sortingOrder = sortingOrder;
        }
    }
}