using Baracuda.Tools;
using Baracuda.Utilities;
using Baracuda.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/Scrollbar Animation")]
    [RequireComponent(typeof(Scrollbar))]
    public class ScrollbarAnimation : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private float colorFadeSharpness = 15f;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color hoverColor = Color.white;
        [SerializeField] private Color scrollColor = Color.white;
        [SerializeField] [Required] private Image targetGraphic;
        [SerializeField] [Required] private Scrollbar scrollbar;
        [SerializeField] [Required] private UISpritesAnimation noiseAnimation;

        [Debug]
        private bool _isSelected;
        [Debug]
        private bool _isHovered;
        [Debug]
        private UnscaledTimer _lastScrollTimer;
        [Debug]
        private Color _targetColor;

        private void OnValidate()
        {
            scrollbar ??= GetComponent<Scrollbar>();
        }

        private void Awake()
        {
            scrollbar.onValueChanged.AddListener(OnScroll);
            _targetColor = defaultColor;
        }

        private void OnDestroy()
        {
            scrollbar.onValueChanged.RemoveListener(OnScroll);
        }

        private void OnScroll(float value)
        {
            _lastScrollTimer = UnscaledTimer.Run(1);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
        }

        private void Update()
        {
            UpdateTargetColor();
            UpdateState();
            this.SetObjectDirty();
        }

        private void UpdateState()
        {
            if (_isSelected || _isHovered || _lastScrollTimer.IsRunning)
            {
                noiseAnimation.Play();
            }
            else
            {
                noiseAnimation.Stop();
            }
        }

        private void UpdateTargetColor()
        {
            if (_isSelected)
            {
                _targetColor = selectedColor;
            }
            else if (_isHovered)
            {
                _targetColor = hoverColor;
            }
            else if (_lastScrollTimer.IsRunning)
            {
                _targetColor = scrollColor;
            }
            else
            {
                _targetColor = defaultColor;
            }

            targetGraphic.color = Color.Lerp(targetGraphic.color, _targetColor, Time.deltaTime * colorFadeSharpness);
        }
    }
}