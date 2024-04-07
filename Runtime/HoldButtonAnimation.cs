using Baracuda.Utilities;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/Hold Button Animation")]
    [RequireComponent(typeof(HoldButton))]
    public class HoldButtonAnimation : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        #region Fields

        [SerializeField] [Required] private HoldButton button;
        [SerializeField] [Required] private Image fillImage;
        [SerializeField] [Required] private Image backgroundImage;
        [SerializeField] [Required] private Image borderImage;
        [SerializeField] [Required] private TMP_Text textField;

        [Header("Fill")]
        [SerializeField] [Required] private AnimationCurve fillCurve;

        [Header("Text")]
        [SerializeField] [Required] private AnimationCurve colorCurve;
        [SerializeField] private Color fromTextColor;
        [SerializeField] private Color toTextColor;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = .2f;
        [SerializeField] private float hoverDuration = .2f;
        [SerializeField] private float selectDuration = .2f;
        [SerializeField] private float recoverDuration = .4f;

        [Header("Border")]
        [SerializeField] private Color borderColorNormal;
        [SerializeField] private Color borderColorSelected;
        [SerializeField] private Color borderColorHover;

        [Header("Background")]
        [SerializeField] private Color backgroundColorNormal;
        [SerializeField] private Color backgroundColorSelected;
        [SerializeField] private Color backgroundColorHover;

        [Header("Text")]
        [SerializeField] private Color textColorNormal;
        [SerializeField] private Color textColorSelected;
        [SerializeField] private Color textColorHover;
        [SerializeField] private float fontSizeNormal = 16;
        [SerializeField] private float fontSizeSelected = 16;
        [SerializeField] private float fontSizeHover = 16;
        [SerializeField] private float characterSpacingNormal = 32;
        [SerializeField] private float characterSpacingSelected = 32;
        [SerializeField] private float characterSpacingHover = 32;

        [Header("Noise")]
        [SerializeField] private UISpritesAnimation noiseAnimation;

        private Sequence _fillSequence;
        private Sequence _sequence;
        private bool _isSelected;
        private bool _isHover;

        #endregion


        #region Setup & Shutdown

        private void Awake()
        {
            button.HoldStarted += OnHoldStarted;
            button.HoldProgress += OnHoldProgress;
            button.HoldCancelled += OnHoldCancelled;
        }

        private void OnDestroy()
        {
            _fillSequence.Stop();
            _sequence.Stop();
            button.HoldStarted -= OnHoldStarted;
            button.HoldProgress -= OnHoldProgress;
            button.HoldCancelled -= OnHoldCancelled;
        }

        private void OnValidate()
        {
            button ??= GetComponent<HoldButton>();
            if (backgroundImage)
            {
                backgroundImage.color = backgroundColorNormal;
            }
            if (borderImage)
            {
                borderImage.color = borderColorNormal;
            }
            if (textField)
            {
                textField.color = textColorNormal;
                textField.fontSize = fontSizeNormal;
                textField.characterSpacing = characterSpacingNormal;
            }
        }

        #endregion


        #region Hold Callbacks

        private void OnHoldStarted()
        {
            FadeToNormal();
        }

        private void OnHoldProgress(float delta)
        {
            var fillProgress = fillCurve.Evaluate(delta);
            fillImage.fillAmount = fillProgress;

            var colorProgress = colorCurve.Evaluate(delta);
            textField.color = Color.Lerp(fromTextColor, toTextColor, colorProgress);
        }

        private void OnHoldCancelled()
        {
            _fillSequence.Stop();
            _fillSequence = Sequence.Create();
            _fillSequence.Chain(Tween.UIFillAmount(fillImage, 0, .3f));
            if (_isSelected)
            {
                FadeToSelected(recoverDuration, Ease.InSine);
                return;
            }
            if (_isHover)
            {
                FadeToHover(recoverDuration, Ease.InSine);
                return;
            }
            FadeToNormal(recoverDuration, Ease.InSine);
        }

        #endregion


        #region Border Color

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            if (button.interactable is false)
            {
                return;
            }
            if (button.IsHoldInProgress)
            {
                return;
            }
            FadeToSelected();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            if (_isHover)
            {
                FadeToHover();
            }
            else
            {
                FadeToNormal();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHover = true;
            if (button.interactable is false)
            {
                return;
            }
            FadeToHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHover = false;
            if (button.interactable is false)
            {
                return;
            }
            if (_isSelected)
            {
                FadeToSelected();
            }
            else
            {
                FadeToNormal();
            }
        }

        #endregion


        #region Fade

        private void FadeToNormal(float? duration = null, Ease? ease = null)
        {
            noiseAnimation.Stop();
            FadeTo(
                borderColorNormal,
                backgroundColorNormal,
                textColorNormal,
                fontSizeNormal,
                characterSpacingNormal,
                duration ?? fadeDuration,
                ease ?? Ease.Default);
        }

        private void FadeToSelected(float? duration = null, Ease? ease = null)
        {
            noiseAnimation.Play();
            FadeTo(
                borderColorSelected,
                backgroundColorSelected,
                textColorSelected,
                fontSizeSelected,
                characterSpacingSelected,
                duration ?? selectDuration,
                ease ?? Ease.Default);
        }

        private void FadeToHover(float? duration = null, Ease? ease = null)
        {
            noiseAnimation.Play();
            FadeTo(
                borderColorHover,
                backgroundColorHover,
                textColorHover,
                fontSizeHover,
                characterSpacingHover,
                duration ?? hoverDuration,
                ease ?? Ease.Default);
        }

        private void FadeTo(Color borderColor, Color backgroundColor, Color fontColor, float fontSize,
            float fontSpacing, float duration, Ease ease)
        {
            if (_sequence.isAlive)
            {
                _sequence.Stop();
            }
            _sequence = Sequence.Create();
            _sequence.Chain(Tween.Color(borderImage, borderColor, duration, ease));
            _sequence.Group(Tween.Color(backgroundImage, backgroundColor, duration, ease));
            _sequence.Group(Tween.Color(textField, fontColor, duration.WithMaxLimit(0.3f), ease));
            _sequence.Group(TextMeshTween.FontSize(textField, fontSize, duration));
            _sequence.Group(TextMeshTween.CharacterSpacing(textField, fontSpacing, duration));
        }

        #endregion
    }
}