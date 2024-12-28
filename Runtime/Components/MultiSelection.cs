using System;
using System.Collections.Generic;
using System.Linq;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Input;
using Baracuda.Utility.Services;
using Baracuda.Utility.Types;
using Baracuda.Utility.Utilities;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Baracuda.UI.Components
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

        private DynamicIndex _index;

        #endregion


        #region Events & Properties

        public bool AllowWrapping { get; set; }
        public bool IsInitialized { get; private set; }
        public int Index => _index;
        public IList<SelectionEntry> Entries { get; private set; }

        public SelectionEntry Entry => Entries[Index];

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

        public void Initialize(IList<SelectionEntry> entries, SelectionEntry startEntry)
        {
            if (IsInitialized)
            {
                Shutdown();
            }

            IsInitialized = true;
            AllowWrapping = allowWrapping;
            Entries = entries;
            startEntry ??= Entries.First();
            _index = DynamicIndex.Create(startEntry.Index, Entries, allowWrapping);
            nextSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnNextPressed;
            previousSelectGraphic.GetOrAddComponent<PointerEvents>().PointerDown += OnPreviousPressed;

            var images = indexWidgetContainer.GetComponentsInChildren<Image>();

            foreach (var element in images)
            {
                element.SetActive(false);
            }

            IndexWidgetsEnabled = entries.Count <= indexDisplayLimit;

            if (IndexWidgetsEnabled)
            {
                IndexWidgets = new Image[entries.Count];

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

            var current = _index.Value;
            _index++;
            if (_index.Value != current)
            {
                RefreshRepresentation();
            }
        }

        public void SelectPrevious()
        {
            if (!IsInitialized)
            {
                return;
            }
            var current = _index.Value;
            _index--;
            if (_index.Value != current)
            {
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

        public void SelectEnumElement(int enumValue)
        {
            if (!IsInitialized)
            {
                return;
            }

            var index = Entries.First(entry => entry.EnumValue == enumValue).Index;
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

            UpdateButtonGraphics();

            ValueChanged?.Invoke(Entries[_index]);
        }

        public void RefreshRepresentationWithoutNotify()
        {
            if (Entries == null)
            {
                return;
            }

            selectionTextField.text = Entries[_index].Name;
            UpdateButtonGraphics();
        }

        public void UpdateButtonGraphics(Color? activeColor = null)
        {
            if (AllowWrapping is false)
            {
                var color = activeColor ?? new Color(0.71f, 0.71f, 0.71f);
                var indexValue = _index.Value;
                if (indexValue < _index.Max())
                {
                    nextSelectGraphic.color = color;
                    nextSelectGraphic.raycastTarget = true;
                }
                else
                {
                    nextSelectGraphic.color = new Color(0.49f, 0.49f, 0.49f, 0.5f);
                    nextSelectGraphic.raycastTarget = false;
                }

                if (indexValue > _index.Min())
                {
                    previousSelectGraphic.color = color;
                    previousSelectGraphic.raycastTarget = true;
                }
                else
                {
                    previousSelectGraphic.color = new Color(0.49f, 0.49f, 0.49f, 0.5f);
                    previousSelectGraphic.raycastTarget = false;
                }
            }
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

            if (interactable is false)
            {
                return;
            }

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
            if (!enabled)
            {
                return;
            }
            if (interactable is false)
            {
                return;
            }
            SelectNext();
        }

        private void OnPreviousPressed(PointerEvents pointerDownEvent)
        {
            if (!enabled)
            {
                return;
            }
            if (interactable is false)
            {
                return;
            }
            SelectPrevious();
        }

        private void OnElementClicked(PointerEvents pointerDownEvent)
        {
            if (!enabled)
            {
                return;
            }
            if (interactable is false)
            {
                return;
            }
            var element = pointerDownEvent.GetComponent<Image>();
            var index = IndexWidgets.IndexOf(element);
            SelectElement(index);
        }

        #endregion
    }
}