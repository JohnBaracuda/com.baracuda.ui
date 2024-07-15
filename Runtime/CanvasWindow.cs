using Baracuda.Bedrock.Injection;
using Baracuda.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CanvasScaleController))]
    public abstract class CanvasWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private UIGroupReference group = UIGroupReference.Menu;
        [SerializeField] [Required] private Canvas canvas;
        [SerializeField] [Required] private CanvasGroup canvasGroup;

        public Canvas Canvas => canvas;
        protected CanvasGroup CanvasGroup => canvasGroup;

        protected virtual void Awake()
        {
            Inject.Dependencies(this, false);
            CanvasGroup.alpha = 0;
        }

        protected virtual void OnDestroy()
        {
            this.DOKill();
        }

        protected void OnValidate()
        {
            canvas ??= GetComponent<Canvas>();
            canvasGroup ??= GetComponent<CanvasGroup>();
        }

        public virtual Sequence ShowAsync(UIContext context)
        {
            this.DOKill();
            this.SetActive(true);
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(1, .3f));
            return sequence;
        }

        public virtual Sequence HideAsync(UIContext context)
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);
            sequence.Append(CanvasGroup.DOFade(0, .3f));
            sequence.AppendCallback(() =>
            {
                Assert.IsNotNull(this);
                this.SetActive(false);
            });

            return sequence;
        }

        public UIGroupReference GetDefaultGroup()
        {
            return group;
        }

        public virtual void SetSortingOrder(int sortingOrder)
        {
            Canvas.sortingOrder = sortingOrder;
        }
    }
}