using Baracuda.Bedrock.PlayerLoop;
using Baracuda.UI.Mediator;
using Baracuda.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(MultiSelection))]
    public class MultiSelectionAnimation : MonoBehaviour
    {
        [Header("Index Widgets")]
        [SerializeField] private Color indexWidgetColor = new(0.18f, 0.18f, 0.18f);
        [SerializeField] private Color indexWidgetActiveColor = Color.white;

        [Header("Buttons")]
        [SerializeField] private Color buttonDefaultColor = Color.white;
        [SerializeField] private Color buttonActiveColor = Color.white;

        [FormerlySerializedAs("selectionDefaultColor")]
        [Header("Selection")]
        [SerializeField] private Color backgroundDefaultColor;
        [FormerlySerializedAs("selectionActiveColor")]
        [SerializeField] private Color backgroundActiveColor;
        [FormerlySerializedAs("selectionTextDefaultColor")]
        [SerializeField] private Color textDefaultColor;
        [FormerlySerializedAs("selectionTextActiveColor")]
        [SerializeField] private Color textActiveColor;
        [SerializeField] private Color noiseActiveColor;
        [SerializeField] private Color noiseDefaultColor;

        [Header("Components")]
        [SerializeField] [Required] private Image backgroundImage;

        [SerializeField] [Required] private TMP_Text selectionTextField;
        [SerializeField] [Required] private UISpritesAnimation noiseAnimation;

        [FormerlySerializedAs("dropdown")]
        [FormerlySerializedAs("option")]
        [SerializeField] [ReadOnly] private MultiSelection multi;

        [SerializeField] [Required] private Image nextGraphic;
        [SerializeField] [Required] private Image previousGraphic;

        private Image _indexTargetGraphic;
        private bool _isHover;
        private bool _isSelected;

        private void OnValidate()
        {
            multi ??= GetComponent<MultiSelection>();
        }

        private void OnEnable()
        {
            multi.PointerEntered += OnHoverStart;
            multi.PointerExited += OnHoverEnd;
            multi.ValueChanged += OnValueChanged;
            multi.Selected += OnSelectionStart;
            multi.Deselected += OnSelectionEnd;
            nextGraphic.GetOrAddComponent<PointerEvents>().PointerEnter += OnNextHoverStart;
            nextGraphic.GetOrAddComponent<PointerEvents>().PointerExit += OnNextHoverEnd;
            previousGraphic.GetOrAddComponent<PointerEvents>().PointerEnter += OnPreviousHoverStart;
            previousGraphic.GetOrAddComponent<PointerEvents>().PointerExit += OnPreviousHoverEnd;

            foreach (var selectionIndexWidget in multi.IndexWidgets)
            {
                selectionIndexWidget.GetOrAddComponent<PointerEvents>().PointerEnter += OnPointerEnter;
                selectionIndexWidget.GetOrAddComponent<PointerEvents>().PointerExit += OnPointerExit;
            }

            if (multi.IsInitialized)
            {
                OnValueChanged(multi.Entry);
            }
        }

        private void OnDisable()
        {
            if (Gameloop.IsQuitting)
            {
                return;
            }

            nextGraphic.GetOrAddComponent<PointerEvents>().PointerEnter -= OnNextHoverStart;
            nextGraphic.GetOrAddComponent<PointerEvents>().PointerExit -= OnNextHoverEnd;
            previousGraphic.GetOrAddComponent<PointerEvents>().PointerEnter -= OnPreviousHoverStart;
            previousGraphic.GetOrAddComponent<PointerEvents>().PointerExit -= OnPreviousHoverEnd;
            multi.Selected -= OnSelectionStart;
            multi.Deselected -= OnSelectionEnd;
            multi.PointerEntered -= OnHoverStart;
            multi.PointerExited -= OnHoverEnd;
            multi.ValueChanged -= OnValueChanged;

            foreach (var selectionIndexWidget in multi.IndexWidgets)
            {
                selectionIndexWidget.GetOrAddComponent<PointerEvents>().PointerEnter -= OnPointerEnter;
                selectionIndexWidget.GetOrAddComponent<PointerEvents>().PointerExit -= OnPointerExit;
            }
        }

        private void OnDestroy()
        {
            backgroundImage.ShutdownTweens();
            selectionTextField.ShutdownTweens();
            this.ShutdownTweens();
        }

        private void OnValueChanged(SelectionEntry entry)
        {
            if (multi.IndexWidgetsEnabled is false)
            {
                return;
            }

            foreach (var indexWidget in multi.IndexWidgets)
            {
                indexWidget.color = indexWidgetColor;
            }

            _indexTargetGraphic = multi.IndexWidgets[entry.Index];
            _indexTargetGraphic.color = indexWidgetActiveColor;
        }

        private void OnPointerEnter(PointerEvents pointerEvents)
        {
            if (pointerEvents.TargetGraphic != null)
            {
                pointerEvents.TargetGraphic.DOColor(indexWidgetActiveColor, .1f);
            }
        }

        private void OnPointerExit(PointerEvents pointerEvents)
        {
            pointerEvents.TargetGraphic.DOColor(_indexTargetGraphic == pointerEvents.TargetGraphic ? indexWidgetActiveColor : indexWidgetColor, .1f);
        }

        private void OnSelectionStart()
        {
            _isSelected = true;
            UpdateRepresentation();
        }

        private void OnSelectionEnd()
        {
            _isSelected = false;
            UpdateRepresentation();
        }

        private void OnHoverStart()
        {
            _isHover = true;
            UpdateRepresentation();
        }

        private void OnHoverEnd()
        {
            _isHover = false;
            UpdateRepresentation();
        }

        private void UpdateRepresentation()
        {
            if (_isHover || _isSelected)
            {
                backgroundImage.DOColor(backgroundActiveColor, .1f);
                selectionTextField.DOColor(textActiveColor, .1f);
                noiseAnimation.SetAnimationColor(noiseActiveColor);
                noiseAnimation.Play();
            }
            else
            {
                backgroundImage.DOColor(backgroundDefaultColor, .1f);
                selectionTextField.DOColor(textDefaultColor, .1f);
                noiseAnimation.SetAnimationColor(noiseDefaultColor);
                noiseAnimation.Stop();
            }
        }


        #region Button Graphics

        private void OnNextHoverStart(PointerEvents pointerEvents)
        {
            var target = pointerEvents.TargetGraphic;
            target.DOKill();
            var sequence = DOTween.Sequence(target);
            sequence.Append(target.transform.DOScale(Vector3.one * 1.1f, .1f).SetEase(Ease.InOutSine));
            sequence.Append(target.DOColor(buttonActiveColor, .3f).SetEase(Ease.InOutSine));
        }

        private void OnNextHoverEnd(PointerEvents pointerEvents)
        {
            var target = pointerEvents.TargetGraphic;
            target.DOKill();
            var sequence = DOTween.Sequence(target);
            sequence.Append(target.transform.DOScale(Vector3.one, .1f).SetEase(Ease.InOutSine));
            sequence.Append(target.DOColor(buttonDefaultColor, .3f).SetEase(Ease.InOutSine));
        }

        private void OnPreviousHoverStart(PointerEvents pointerEvents)
        {
            var target = pointerEvents.TargetGraphic;
            target.DOKill();
            var sequence = DOTween.Sequence(target);
            sequence.Append(target.transform.DOScale(Vector3.one * 1.1f, .1f).SetEase(Ease.InOutSine));
            sequence.Append(target.DOColor(buttonActiveColor, .3f).SetEase(Ease.InOutSine));
        }

        private void OnPreviousHoverEnd(PointerEvents pointerEvents)
        {
            var target = pointerEvents.TargetGraphic;
            target.DOKill();
            var sequence = DOTween.Sequence(target);
            sequence.Append(target.transform.DOScale(Vector3.one, .1f).SetEase(Ease.InOutSine));
            sequence.Append(target.DOColor(buttonDefaultColor, .3f).SetEase(Ease.InOutSine));
        }

        #endregion
    }
}