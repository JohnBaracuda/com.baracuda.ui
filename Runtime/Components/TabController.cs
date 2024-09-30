using System;
using Baracuda.Utility.Types;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Baracuda.UI.Components
{
    public class TabController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TabEntry[] tabs;
        [SerializeField] private InputActionReference nextTabInput;
        [SerializeField] private InputActionReference previousTabInput;

        public event Action<TabEntry> ActiveTabChanged;

        [Serializable]
        public struct TabEntry
        {
            public ButtonAnimation button;
            public Tab tab;
        }

        private DynamicIndex _tabIndex;
        private bool _isInitialized;

        private Tab GetTabByIndex(int index)
        {
            return tabs[index].tab;
        }

        private ButtonAnimation GetButtonByIndex(int index)
        {
            return tabs[index].button;
        }

        private TabEntry GetByIndex(int index)
        {
            return tabs[index];
        }

        #endregion


        #region API

        public TabEntry[] TabEntries => tabs;

        public void OpenNextTab()
        {
            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);

            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.button.Unlock();
            _tabIndex++;
            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.button.Lock();
            EventSystem.current.SetSelectedGameObject(next.button.gameObject);
            ActiveTabChanged?.Invoke(next);
        }

        public void OpenPreviousTab()
        {
            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);

            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.button.Unlock();
            _tabIndex--;
            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.button.Lock();
            EventSystem.current.SetSelectedGameObject(next.button.gameObject);
            ActiveTabChanged?.Invoke(next);
        }

        public void OpenTab(int index)
        {
            if (_tabIndex.Value == index)
            {
                return;
            }

            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);
            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.button.Unlock();

            _tabIndex.Value = index;

            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.button.Lock();
            ActiveTabChanged?.Invoke(next);
        }

        public TabEntry CurrentTab
        {
            get
            {
                Initialize();
                return GetByIndex(_tabIndex);
            }
        }

        #endregion


        #region Setup & Initialization

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            for (var index = 0; index < tabs.Length; index++)
            {
                var tabEntry = tabs[index];
                var button = tabEntry.button;
                var capturedIndex = index;
                button.Button.onClick.AddListener(() => OpenTab(capturedIndex));
                button.Selected += () => OpenTab(capturedIndex);
                var tab = tabEntry.tab;
                tab.FadeOut().Complete(true);
            }

            _tabIndex = DynamicIndex.Create(tabs);

            var currentTab = GetTabByIndex(_tabIndex);
            currentTab.FadeIn().Complete(true);
            var currentButton = GetButtonByIndex(_tabIndex);
            currentButton.Lock();
        }

        private void OnDestroy()
        {
            this.ShutdownTweens();
        }

        #endregion


        #region Input

        private void OnEnable()
        {
            nextTabInput.action.performed += OnNextTabInput;
            previousTabInput.action.performed += OnPreviousTabInput;
            CurrentTab.button.Lock();
        }

        private void OnDisable()
        {
            nextTabInput.action.performed -= OnNextTabInput;
            previousTabInput.action.performed -= OnPreviousTabInput;
        }

        private void OnNextTabInput(InputAction.CallbackContext context)
        {
            OpenNextTab();
        }

        private void OnPreviousTabInput(InputAction.CallbackContext context)
        {
            OpenPreviousTab();
        }

        #endregion
    }
}