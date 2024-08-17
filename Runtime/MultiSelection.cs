using System;
using System.Linq;
using Baracuda.Bedrock.Collections;
using Baracuda.Bedrock.Input;
using Baracuda.Bedrock.Services;
using Baracuda.Bedrock.Types;
using Baracuda.Bedrock.Utilities;
using Baracuda.UI.Mediator;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Baracuda.UI
{
    public class MultiSelection : Selectable,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
    {
        #region Settings

        [SerializeField] private bool allowWrapping;
        [SerializeField] private int indexDisplayLimit = 32;

        [Header("Components")]
        [SerializeField] [Required] private Image nextSelectGraphic;

        [SerializeField] [Required] private Image previousSelectGraphic;
        [SerializeField] [Required] private TMP_Text selectionTextField;
        [SerializeField] [Required] private Transform indexWidgetContainer;

        #endregion


        #region Fields

        private Loop _index;

        #endregion


        #region Events & Properties

        public bool AllowWrapping { get; set; }
        public bool IsInitialized { get; private set; }
        public int Index => _index;
        public SelectionEntry[] Entries { get; private set; }

        public SelectionEntry Entry => Entries[_index];

        public event Action PointerEntered;
        public event Action PointerExited;
        public event Action Selected;

        public event Action Deselected;

        // Index and entry
        public event Action<SelectionEntry> ValueChanged;

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

        public void Initialize(SelectionEntry[] entries, SelectionEntry startEntry)
        {
            if (IsInitialized)
            {
                Shutdown();
            }

            IsInitialized = true;
            AllowWrapping = allowWrapping;
            Entries = entries;
            startEntry ??= Entries.First();
            _index = Loop.Create(startEntry.Index, Entries);
            nextSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnNextPressed;
            previousSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnPreviousPressed;

            var images = indexWidgetContainer.GetComponentsInChildren<Image>();

            foreach (var element in images)
            {
                element.SetActive(false);
            }

            IndexWidgetsEnabled = entries.Length <= indexDisplayLimit;

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
            if (!IsInitialized)
            {
                return;
            }

            if (_index.IsMax is false || AllowWrapping)
            {
                _index++;
                RefreshRepresentation();
            }
        }

        public void SelectPrevious()
        {
            if (!IsInitialized)
            {
                return;
            }

            if (_index.IsMin is false || AllowWrapping)
            {
                _index--;
                RefreshRepresentation();
            }
        }

        public void SelectElement(int index)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (_index.Value == index)
            {
                return;
            }

            if (index == -1)
            {
                index = RandomUtility.Index(Entries);
            }

            _index.Value = index;
            RefreshRepresentation();
        }

        public void RefreshRepresentation()
        {
            if (Entries == null)
            {
                return;
            }

            selectionTextField.text = Entries[_index].Name;
            ValueChanged?.Invoke(Entries[_index]);
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