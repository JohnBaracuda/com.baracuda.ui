using Baracuda.Utilities.Types;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
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

        private bool _isForceSelected;

        public bool IsSelected { get; private set; }
        public bool IsHovered { get; private set; }
        public event Action Selected;
        public event Action Deselected;

        #endregion


        #region Setup & Shutdown

        private void OnDestroy()
        {
            this.DOKill();
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

        public void ForceSelect()
        {
            _isForceSelected = true;
            FadeToSelected();
        }

        public void ForceDeselect()
        {
            _isForceSelected = false;
            if (IsSelected)
            {
                FadeToSelected();
                return;
            }
            if (IsHovered)
            {
                FadeToHover();
                return;
            }
            FadeToNormal();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke();
            IsSelected = true;
            if (targetButton.interactable is false)
            {
                return;
            }
            if (_isForceSelected)
            {
                return;
            }
            FadeToSelected();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Deselected?.Invoke();
            IsSelected = false;
            if (_isForceSelected)
            {
                return;
            }
            FadeToNormal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            if (targetButton.interactable is false)
            {
                return;
            }
            if (_isForceSelected)
            {
                return;
            }
            FadeToHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            if (_isForceSelected)
            {
                return;
            }
            if (IsSelected)
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
            this.DOKill();

            var sequence = DOTween.Sequence(this);

            // Animate the colors of the border and background images
            sequence.Join(borderImage.DOColor(borderColor, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColor, fadeDuration));
            sequence.Join(textField.DOColor(fontColor, fadeDuration));

            // Animate the font size and character spacing of the text field
            sequence.Join(textField.DOFontSize(fontSize, fadeDuration));
            sequence.Join(textField.DOTextMeshProSpacing(fontSpacing, fadeDuration));
        }

        #endregion
    }
}