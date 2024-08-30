using System;
using Baracuda.Bedrock.PlayerLoop;
using Baracuda.Bedrock.Types;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
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

        [Header("Transition")]
        [SerializeField] private float fadeDuration = .2f;

        [Header("Clicked")]
        [SerializeField] private Color borderColorClicked;
        [SerializeField] private Color backgroundColorClicked;
        [SerializeField] private Color animationColorClicked;
        [SerializeField] private Color textColorClicked;
        [SerializeField] private Ease clickEase = Ease.InBounce;
        [SerializeField] private float clickDuration = .2f;

        [Header("Default")]
        [SerializeField] private Color borderColorNormal;
        [SerializeField] private Color backgroundColorNormal;
        [SerializeField] private Color animationColorNormal;
        [SerializeField] private Color textColorNormal;
        [SerializeField] private float fontSizeNormal = 16;
        [SerializeField] private float characterSpacingNormal = 32;

        [Header("Selected")]
        [SerializeField] private Color borderColorSelected;
        [SerializeField] private Color backgroundColorSelected;
        [SerializeField] private Color animationColorSelected;
        [SerializeField] private Color textColorSelected;
        [SerializeField] private float fontSizeSelected = 16;
        [SerializeField] private float characterSpacingSelected = 32;

        [Header("Hover")]
        [SerializeField] private Color borderColorHover;
        [SerializeField] private Color backgroundColorHover;
        [SerializeField] private Color animationColorHover;
        [SerializeField] private Color textColorHover;
        [SerializeField] private float fontSizeHover = 16;
        [SerializeField] private float characterSpacingHover = 32;

        [Header("Locked")]
        [SerializeField] private Color borderColorLocked;
        [SerializeField] private Color backgroundColorLocked;
        [SerializeField] private Color animationColorLocked;
        [SerializeField] private Color textColorLocked;
        [SerializeField] private float fontSizeLocked = 16;
        [SerializeField] private float characterSpacingLocked = 32;

        [Header("Components")]
        [SerializeField] [Required] private Button targetButton;
        [SerializeField] [Required] private Image borderImage;
        [SerializeField] [Required] private Image backgroundImage;
        [SerializeField] [Required] private TMP_Text textField;

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
        public event Action Hovered;
        public event Action Unhovered;
        public Button Button => targetButton;

        #endregion


        #region Setup & Shutdown

        private void Awake()
        {
            targetButton.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            targetButton.onClick.RemoveListener(OnButtonClick);
            this.ShutdownTweens();
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


        #region Click Animation

        private void OnButtonClick()
        {
            this.DOKill();
            var sequence = DOTween.Sequence(this);

            // Animate the colors of the border and background images
            sequence.Join(borderImage.DOColor(borderColorClicked, clickDuration).SetEase(clickEase));
            sequence.Join(backgroundImage.DOColor(backgroundColorClicked, clickDuration).SetEase(clickEase));
            sequence.Join(textField.DOColor(textColorClicked, clickDuration).SetEase(clickEase));

            // Animate the font size and character spacing of the text field
            sequence.Join(textField.DOFontSize(textField.fontSize - 2, clickDuration).SetEase(clickEase));
            sequence.Join(textField.DOCharacterSpacing(textField.characterSpacing + 1, clickDuration).SetEase(clickEase));

            sequence.AppendCallback(UpdateRepresentation);
        }

        #endregion


        #region Logic

        public void Lock()
        {
            IsLocked = true;
            UpdateRepresentation();
        }

        public void Unlock()
        {
            IsLocked = false;
            UpdateRepresentation();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke();
            IsSelected = true;
            UpdateRepresentation();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Deselected?.Invoke();
            IsSelected = false;
            UpdateRepresentation();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hovered?.Invoke();
            IsHovered = true;
            UpdateRepresentation();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unhovered?.Invoke();
            IsHovered = false;
            UpdateRepresentation();
        }

        private void UpdateRepresentation()
        {
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

            if (IsHovered)
            {
                FadeToHover();
                return;
            }

            FadeToNormal();
        }

        private void FadeToNormal()
        {
            textField.font = normalFont;
            noiseAnimation.SetAnimationColor(animationColorNormal);
            noiseAnimation.Stop();
            this.ShutdownTweens();

            var sequence = DOTween.Sequence(this);

            sequence.Join(borderImage.DOColor(borderColorNormal, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColorNormal, fadeDuration));
            sequence.Join(textField.DOColor(textColorNormal, fadeDuration));
            sequence.Join(textField.DOFontSize(fontSizeNormal, fadeDuration));
            sequence.Join(textField.DOCharacterSpacing(characterSpacingNormal, fadeDuration));
        }

        private void FadeToSelected()
        {
            if (selectedFont.Enabled)
            {
                textField.font = selectedFont.Value;
            }

            noiseAnimation.SetAnimationColor(animationColorSelected);
            noiseAnimation.Play();
            this.ShutdownTweens();

            var sequence = DOTween.Sequence(this);

            sequence.Join(borderImage.DOColor(borderColorSelected, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColorSelected, fadeDuration));
            sequence.Join(textField.DOColor(textColorSelected, fadeDuration));
            sequence.Join(textField.DOFontSize(fontSizeSelected, fadeDuration));
            sequence.Join(textField.DOCharacterSpacing(characterSpacingSelected, fadeDuration));
        }

        private void FadeToHover()
        {
            if (hoverFont.Enabled)
            {
                textField.font = hoverFont.Value;
            }

            noiseAnimation.SetAnimationColor(animationColorHover);
            noiseAnimation.Play();
            this.ShutdownTweens();

            var sequence = DOTween.Sequence(this);

            sequence.Join(borderImage.DOColor(borderColorHover, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColorHover, fadeDuration));
            sequence.Join(textField.DOColor(textColorHover, fadeDuration));
            sequence.Join(textField.DOFontSize(fontSizeHover, fadeDuration));
            sequence.Join(textField.DOCharacterSpacing(characterSpacingHover, fadeDuration));
        }

        private void FadeToLocked()
        {
            if (lockedFont.Enabled)
            {
                textField.font = lockedFont.Value;
            }

            noiseAnimation.SetAnimationColor(animationColorLocked);
            noiseAnimation.Play();
            this.ShutdownTweens();

            var sequence = DOTween.Sequence(this);

            sequence.Join(borderImage.DOColor(borderColorLocked, fadeDuration));
            sequence.Join(backgroundImage.DOColor(backgroundColorLocked, fadeDuration));
            sequence.Join(textField.DOColor(textColorLocked, fadeDuration));
            sequence.Join(textField.DOFontSize(fontSizeLocked, fadeDuration));
            sequence.Join(textField.DOCharacterSpacing(characterSpacingLocked, fadeDuration));
        }

        #endregion
    }
}