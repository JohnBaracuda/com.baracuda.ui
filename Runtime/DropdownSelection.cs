using Baracuda.Bedrock.Services;
using Baracuda.UI.Mediator;
using Baracuda.Utilities;
using Baracuda.Utilities.Types;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Baracuda.UI
{
    public class DropdownSelection : Selectable,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
    {
        #region Settings

        [SerializeField] private bool allowWrapping;

        [Header("Components")]
        [SerializeField] [Required] private Image nextSelectGraphic;
        [SerializeField] [Required] private Image previousSelectGraphic;
        [SerializeField] [Required] private TMP_Text selectionTextField;
        [SerializeField] [Required] private Transform indexWidgetContainer;

        #endregion


        #region Fields

        private Loop _index;
        private DropdownEntry[] _entries;

        #endregion


        #region Events & Properties

        public bool AllowWrapping { get; set; }
        public bool IsInitialized { get; private set; }
        public int Index => _index;
        public DropdownEntry Entry => _entries[_index];

        public event Action PointerEntered;
        public event Action PointerExited;
        public event Action Selected;
        public event Action Deselected;
        // Index and entry
        public event Action<DropdownEntry> ValueChanged;

        public bool IndexWidgetsEnabled { get; private set; }
        public Image[] IndexWidgets { get; private set; } = Array.Empty<Image>();

        #endregion


        #region Event Functions

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Shutdown();
        }

        #endregion


        #region Setup

        public void Initialize(DropdownEntry[] entries, DropdownEntry startEntry)
        {
            if (IsInitialized)
            {
                Shutdown();
            }
            IsInitialized = true;
            AllowWrapping = allowWrapping;
            _entries = entries;
            startEntry ??= _entries.First();
            _index = Loop.Create(startEntry.Index, _entries);
            nextSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnNextPressed;
            previousSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnPreviousPressed;

            var images = indexWidgetContainer.GetComponentsInChildren<Image>();
            foreach (var element in images)
            {
                element.SetActive(false);
            }

            IndexWidgetsEnabled = entries.Length <= 32;

            if (IndexWidgetsEnabled)
            {
                IndexWidgets = new Image[entries.Length];
                for (var index = 0; index < IndexWidgets.Length; index++)
                {
                    var widget = images.Length - 1 >= index
                        ? images[index]
                        : Instantiate(images[0], indexWidgetContainer);

                    IndexWidgets[index] = widget;
                    widget.SetActive(true);
                    widget.GetOrAddComponent<PointerEvents>().PointerDown += OnElementClicked;
                }
            }

            RefreshRepresentation();
        }

        private void Shutdown()
        {
            nextSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown -= OnNextPressed;
            previousSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown -= OnPreviousPressed;

            foreach (var indexWidget in IndexWidgets)
            {
                indexWidget.GetOrAddComponent<PointerEvents>().PointerDown -= OnElementClicked;
            }
        }

        #endregion


        #region Selection Callbacks

        public void SelectNext()
        {
            if (_index.IsMax is false || AllowWrapping)
            {
                _index++;
                RefreshRepresentation();
            }
        }

        public void SelectPrevious()
        {
            if (_index.IsMin is false || AllowWrapping)
            {
                _index--;
                RefreshRepresentation();
            }
        }

        public void SelectElement(int index)
        {
            if (_index.Value == index)
            {
                return;
            }
            _index.Value = index;
            RefreshRepresentation();
        }

        public void RefreshRepresentation()
        {
            if (_entries == null)
            {
                return;
            }
            selectionTextField.text = _entries[_index].Name;
            ValueChanged?.Invoke(_entries[_index]);
        }

        #endregion


        #region Pointer Callbacks

        public void OnSubmit(BaseEventData eventData)
        {
            if (ServiceLocator.Get<InputManager>().InteractionMode is InteractionMode.NavigationInput)
            {
                SelectNext();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ServiceLocator.Get<InputManager>().InteractionMode is InteractionMode.NavigationInput)
            {
                SelectNext();
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            PointerEntered?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            PointerExited?.Invoke();
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            if (eventData.moveDir is MoveDirection.Right)
            {
                SelectNext();
                return;
            }
            if (eventData.moveDir is MoveDirection.Left)
            {
                SelectPrevious();
            }
        }

        #endregion


        #region Selection Callbacks

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Selected?.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Deselected?.Invoke();
        }

        #endregion


        #region Button Callbacks

        private void OnNextPressed(PointerEvents pointerDownEvent)
        {
            SelectNext();
        }

        private void OnPreviousPressed(PointerEvents pointerDownEvent)
        {
            SelectPrevious();
        }

        private void OnElementClicked(PointerEvents pointerDownEvent)
        {
            var element = pointerDownEvent.GetComponent<Image>();
            var index = IndexWidgets.IndexOf(element);
            SelectElement(index);
        }

        #endregion
    }
}