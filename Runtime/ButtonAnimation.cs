using Baracuda.Bedrock.PlayerLoop;
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
        [SerializeField] private Color borderColorLocked;

        [Header("Background")]
        [SerializeField] [Required] private Image backgroundImage;
        [SerializeField] private Color backgroundColorNormal;
        [SerializeField] private Color backgroundColorSelected;
        [SerializeField] private Color backgroundColorHover;
        [SerializeField] private Color backgroundColorLocked;
        [SerializeField] private Color animationColorNormal;
        [SerializeField] private Color animationColorSelected;
        [SerializeField] private Color animationColorHover;
        [SerializeField] private Color animationColorLocked;

        [Header("Text")]
        [SerializeField] [Required] private TMP_Text textField;
        [SerializeField] private Color textColorNormal;
        [SerializeField] private Color textColorSelected;
        [SerializeField] private Color textColorHover;
        [SerializeField] private Color textColorLocked;
        [SerializeField] private float fontSizeNormal = 16;
        [SerializeField] private float fontSizeSelected = 16;
        [SerializeField] private float fontSizeHover = 16;
        [SerializeField] private float fontSizeLocked = 16;
        [SerializeField] private float characterSpacingNormal = 32;
        [SerializeField] private float characterSpacingSelected = 32;
        [SerializeField] private float characterSpacingHover = 32;
        [SerializeField] private float characterSpacingLocked = 32;

        [Header("Font Assets")]
        [SerializeField] private TMP_FontAsset normalFont;
        [SerializeField] private Optional<TMP_FontAsset> hoverFont;
        [SerializeField] private Optional<TMP_FontAsset> selectedFont;
        [SerializeField] private Optional<TMP_FontAsset> lockedFont;

        [Header("Noise")]
        [SerializeField] private UISpritesAnimation noiseAnimation;

        public bool IsSelected { get; private set; }
        public bool IsHovered { get; private set; }
        public bool IsLocked { get; private set; }
        public event Action Selected;
        public event Action Deselected;
        public Button Button => targetButton;

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

        private void OnDisable()
        {
            if (Gameloop.IsQuitting)
            {
                return;
            }
            IsLocked = false;
            IsSelected = false;
            IsHovered = false;
            FadeToNormal();
        }

        #endregion


        #region Logic

        public void Lock()
        {
            IsLocked = true;
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
            FadeToLocked();
        }

        public void Unlock()
        {
            IsLocked = false;
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
            FadeToSelected();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Deselected?.Invoke();
            IsSelected = false;
            if (IsLocked)
            {
                FadeToLocked();
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
            FadeToHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            if (IsSelected)
            {
                FadeToSelected();
                return;
            }
            if (IsLocked)
            {
                FadeToLocked();
                return;
            }
            FadeToNormal();
        }

        private void FadeToNormal()
        {
            textField.font = normalFont;
            noiseAnimation.SetAnimationColor(animationColorNormal);
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
            noiseAnimation.SetAnimationColor(animationColorSelected);
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
            noiseAnimation.SetAnimationColor(animationColorHover);
            noiseAnimation.Play();
            FadeTo(
                borderColorHover,
                backgroundColorHover,
                textColorHover,
                fontSizeHover,
                characterSpacingHover);
        }

        private void FadeToLocked()
        {
            if (lockedFont.Enabled)
            {
                textField.font = lockedFont.Value;
            }
            noiseAnimation.SetAnimationColor(animationColorLocked);
            noiseAnimation.Play();
            FadeTo(
                borderColorLocked,
                backgroundColorLocked,
                textColorLocked,
                fontSizeLocked,
                characterSpacingLocked);
        }

        private void FadeTo(
            Color borderColor,
            Color backgroundColor,
            Color textColor,
            float fontSize,
            float fontSpacing)
        {
            this.DOKill();

            var sequence = DOTween.Sequence(this);

            // Animate the colors of the border and background images
            sequence.Join(borderImage.DOColor(borderColor, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColor, fadeDuration));
            sequence.Join(textField.DOColor(textColor, fadeDuration));

            // Animate the font size and character spacing of the text field
            sequence.Join(textField.DOFontSize(fontSize, fadeDuration));
            sequence.Join(textField.DOCharacterSpacing(fontSpacing, fadeDuration));
        }

        #endregion
    }
}