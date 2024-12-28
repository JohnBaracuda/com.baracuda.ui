using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baracuda.Utility.Types;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Baracuda.UI.Components
{
    public class TabController : MonoBehaviour
    {
        #region Public API

        [PublicAPI]
        public IReadOnlyList<TabEntry> TabEntries => _tabs;

        [PublicAPI]
        public TabEntry CurrentTab => GetCurrentTab();

        [PublicAPI]
        public void OpenNextTab()
        {
            OpenNextTabInternal();
        }

        [PublicAPI]
        public void OpenPreviousTab()
        {
            OpenPreviousTabInternal();
        }

        [PublicAPI]
        public void OpenTab(int index)
        {
            OpenTabInternal(index);
        }

        [PublicAPI]
        public void AddTabEntry(TabEntry tabEntry)
        {
            AddTabEntryInternal(tabEntry);
        }

        [PublicAPI]
        public bool Initialize()
        {
            return InitializeInternal();
        }

        #endregion


        #region Fields

        [SerializeField] private bool initializeInStart = true;
        [SerializeField] private TabEntry[] tabs;
        [SerializeField] private InputActionReference nextTabInput;
        [SerializeField] private InputActionReference previousTabInput;

        private readonly List<TabEntry> _tabs = new();

        public event Action<TabEntry> ActiveTabChanged;

        [Serializable]
        public struct TabEntry
        {
            [FormerlySerializedAs("button")]
            public ButtonAnimation tabButton;
            public Tab tab;

            public T GetTab<T>() where T : Tab
            {
                return tab as T;
            }
        }

        private DynamicIndex _tabIndex;
        private bool _isInitialized;

        private Tab GetTabByIndex(int index)
        {
            return _tabs[index].tab;
        }

        private ButtonAnimation GetButtonByIndex(int index)
        {
            return _tabs[index].tabButton;
        }

        private TabEntry GetByIndex(int index)
        {
            return _tabs[index];
        }

        #endregion


        #region Logic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TabEntry GetCurrentTab()
        {
            InitializeInternal();
            return GetByIndex(_tabIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenNextTabInternal()
        {
            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);

            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.tabButton.Unlock();
            _tabIndex++;
            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.tabButton.Lock();
            EventSystem.current.SetSelectedGameObject(next.tabButton.gameObject);
            ActiveTabChanged?.Invoke(next);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenPreviousTabInternal()
        {
            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);

            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.tabButton.Unlock();
            _tabIndex--;
            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.tabButton.Lock();
            EventSystem.current.SetSelectedGameObject(next.tabButton.gameObject);
            ActiveTabChanged?.Invoke(next);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OpenTabInternal(int index)
        {
            if (_tabIndex.Value == index)
            {
                return;
            }

            this.DOComplete(true);
            var sequence = DOTween.Sequence(this);
            var current = GetByIndex(_tabIndex);
            sequence.Append(current.tab.FadeOut());
            current.tabButton.Unlock();

            _tabIndex.Value = index;

            var next = GetByIndex(_tabIndex);
            sequence.Append(next.tab.FadeIn());
            next.tabButton.Lock();
            ActiveTabChanged?.Invoke(next);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddTabEntryInternal(TabEntry tabEntry)
        {
            _tabs.Add(tabEntry);
        }

        #endregion


        #region Setup & Initialization

        private void Awake()
        {
            _tabs.AddRange(tabs);
        }

        private void Start()
        {
            if (initializeInStart)
            {
                InitializeInternal();
            }
        }

        private bool InitializeInternal()
        {
            if (_isInitialized)
            {
                return false;
            }
            _isInitialized = true;
            for (var index = 0; index < _tabs.Count; index++)
            {
                var tabEntry = _tabs[index];
                var button = tabEntry.tabButton;
                var capturedIndex = index;
                button.Button.onClick.AddListener(() => OpenTab(capturedIndex));
                button.Selected += () => OpenTab(capturedIndex);
                var tab = tabEntry.tab;
                tab.FadeOut().Complete(true);
            }

            _tabIndex = DynamicIndex.Create(_tabs);

            var currentTab = GetTabByIndex(_tabIndex);
            currentTab.FadeIn().Complete(true);
            var currentButton = GetButtonByIndex(_tabIndex);
            currentButton.Lock();
            return true;
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
            if (_isInitialized)
            {
                CurrentTab.tabButton.Lock();
            }
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