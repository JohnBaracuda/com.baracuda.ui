using Baracuda.Utilities.Types;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [AddComponentMenu("UI/Button Animation")]
    [RequireComponent(typeof(Button))]
    public class ButtonAnimation : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        #region Fields

        [SerializeField] private Button targetButton;
        [SerializeField] private float fadeDuration = .2f;
        [Header("Border")]
        [SerializeField] [Required] private Image borderImage;
        [SerializeField] private Color borderColorNormal;
        [SerializeField] private Color borderColorSelected;
        [SerializeField] private Color borderColorHover;
        [Header("Background")]
        [SerializeField] [Required] private Image backgroundImage;
        [SerializeField] private Color backgroundColorNormal;
        [SerializeField] private Color backgroundColorSelected;
        [SerializeField] private Color backgroundColorHover;
        [Header("Text")]
        [SerializeField] [Required] private TMP_Text textField;
        [SerializeField] private Color textColorNormal;
        [SerializeField] private Color textColorSelected;
        [SerializeField] private Color textColorHover;
        [SerializeField] private float fontSizeNormal = 16;
        [SerializeField] private float fontSizeSelected = 16;
        [SerializeField] private float fontSizeHover = 16;
        [SerializeField] private float characterSpacingNormal = 32;
        [SerializeField] private float characterSpacingSelected = 32;
        [SerializeField] private float characterSpacingHover = 32;
        [Header("Font Assets")]
        [SerializeField] private TMP_FontAsset normalFont;
        [SerializeField] private Optional<TMP_FontAsset> hoverFont;
        [SerializeField] private Optional<TMP_FontAsset> selectedFont;
        [Header("Noise")]
        [SerializeField] private UISpritesAnimation noiseAnimation;

        private Sequence _sequence;
        private bool _isSelected;

        #endregion


        #region Setup & Shutdown

        private void OnDestroy()
        {
            _sequence.Stop();
        }

        private void OnValidate()
        {
            targetButton ??= GetComponent<Button>();
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
                normalFont ??= textField.font;
                textField.font = normalFont;
                textField.color = textColorNormal;
                textField.fontSize = fontSizeNormal;
                textField.characterSpacing = characterSpacingNormal;
            }
        }

        #endregion


        #region Logic

        public void OnSelect(BaseEventData eventData)
        {
            if (targetButton.interactable is false)
            {
                return;
            }
            _isSelected = true;
            FadeToSelected();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            FadeToNormal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (targetButton.interactable is false)
            {
                return;
            }
            FadeToHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected)
            {
                FadeToSelected();
            }
            else
            {
                FadeToNormal();
            }
        }

        private void FadeToNormal()
        {
            textField.font = normalFont;
            noiseAnimation.Stop();
            FadeTo(
                borderColorNormal,
                backgroundColorNormal,
                textColorNormal,
                fontSizeNormal,
                characterSpacingNormal);
        }

        private void FadeToSelected()
        {
            if (selectedFont.Enabled)
            {
                textField.font = selectedFont.Value;
            }
            noiseAnimation.Play();
            FadeTo(
                borderColorSelected,
                backgroundColorSelected,
                textColorSelected,
                fontSizeSelected,
                characterSpacingSelected);
        }

        private void FadeToHover()
        {
            if (hoverFont.Enabled)
            {
                textField.font = hoverFont.Value;
            }
            noiseAnimation.Play();
            FadeTo(
                borderColorHover,
                backgroundColorHover,
                textColorHover,
                fontSizeHover,
                characterSpacingHover);
        }

        private void FadeTo(Color borderColor, Color backgroundColor, Color fontColor, float fontSize,
            float fontSpacing)
        {
            _sequence.Stop();
            _sequence = Sequence.Create();
            _sequence.Chain(Tween.Color(borderImage, borderColor, fadeDuration));
            _sequence.Group(Tween.Color(backgroundImage, backgroundColor, fadeDuration));
            _sequence.Group(Tween.Color(textField, fontColor, fadeDuration));
            _sequence.Group(TextMeshTween.FontSize(textField, fontSize, fadeDuration));
            _sequence.Group(TextMeshTween.CharacterSpacing(textField, fontSpacing, fadeDuration));
        }

        #endregion
    }
}