using Baracuda.Utility.Utilities;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(Image))]
    public class UIBackground : MonoBehaviour
    {
        [SerializeField] private Color color = new(0f, 0f, 0f, 0.9f);
        [SerializeField] private float fadeInDuration = .4f;
        [SerializeField] private float fadeOutDuration = .3f;
        [SerializeField] [Required] private Image image;
        [SerializeField] [Required] private Canvas canvas;

        private bool _isVisible;

        private void Awake()
        {
            image.color = image.color.WithAlpha(0);
            _isVisible = false;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            canvas.sortingOrder = sortingOrder;
        }

        public void Show()
        {
            if (_isVisible)
            {
                return;
            }
            _isVisible = true;
            image.DOKill();
            image.DOColor(color, fadeInDuration);
        }

        public void Hide()
        {
            if (!_isVisible)
            {
                return;
            }
            _isVisible = false;
            image.DOKill();
            image.DOColor(Color.clear, fadeOutDuration);
        }
    }
}