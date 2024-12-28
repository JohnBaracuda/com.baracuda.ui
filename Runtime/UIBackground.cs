using Baracuda.Utility.Collections;
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

        private readonly ObservableHashSet<object> _showBackgroundSources = new();
        private readonly ObservableHashSet<object> _blockBackgroundSources = new();
        private bool _isVisible;

        private void Awake()
        {
            image.color = image.color.WithAlpha(0);
            _isVisible = _blockBackgroundSources.IsEmpty && _showBackgroundSources.IsNotEmpty;
            _showBackgroundSources.FirstAdded.AddListener(Evaluate);
            _showBackgroundSources.LastRemoved.AddListener(Evaluate);
            _blockBackgroundSources.FirstAdded.AddListener(Evaluate);
            _blockBackgroundSources.LastRemoved.AddListener(Evaluate);
        }

        private void OnDestroy()
        {
            _showBackgroundSources.FirstAdded.RemoveListener(Evaluate);
            _showBackgroundSources.LastRemoved.RemoveListener(Evaluate);
            _blockBackgroundSources.FirstAdded.RemoveListener(Evaluate);
            _blockBackgroundSources.LastRemoved.RemoveListener(Evaluate);
            image.DOKill();
        }

        public void SetSortingOrder(int sortingOrder)
        {
            canvas.sortingOrder = sortingOrder;
        }

        public void Show(object source)
        {
            _showBackgroundSources.Add(source);
        }

        public void Hide(object source)
        {
            _showBackgroundSources.Remove(source);
        }

        public void Block(object source)
        {
            _showBackgroundSources.Add(source);
        }

        public void UnBlock(object source)
        {
            _showBackgroundSources.Remove(source);
        }

        private void Evaluate()
        {
            var wasVisible = _isVisible;
            _isVisible = _blockBackgroundSources.IsEmpty && _showBackgroundSources.IsNotEmpty;
            if (wasVisible == _isVisible)
            {
                return;
            }

            if (_isVisible)
            {
                image.DOKill();
                image.DOColor(color, fadeInDuration);
            }
            else
            {
                image.DOKill();
                image.DOColor(Color.clear, fadeOutDuration);
            }
        }
    }
}