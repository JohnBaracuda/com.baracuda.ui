using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Baracuda.UI.Selection;
using Baracuda.Utility.Collections;
using Baracuda.Utility.Pools;
using Baracuda.Utility.Services;
using Baracuda.Utility.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baracuda.UI.Components
{
    public class SelectionRouter : Selectable
    {
        [SerializeField] private bool updateGameObjectName;
        [SerializeField] private bool autoPrioritize;
        [SerializeField] private bool allowReselection;
        [SerializeField] private Selectable[] targets;

        private readonly List<Selectable> _targets = new();
        private SelectionManager _selectionManager;

        protected override void Awake()
        {
            base.Awake();
            _targets.AddRange(targets ?? Array.Empty<Selectable>());
            if (Application.isPlaying)
            {
                ServiceLocator.Inject(ref _selectionManager);
            }
        }

        public void Configure(params Selectable[] selectableTargets)
        {
            ServiceLocator.Inject(ref _selectionManager);
            _targets.Clear();
            _targets.AddRange(selectableTargets);
            UpdateGameObjectName();
        }

        public void Add(Selectable selectable)
        {
            _targets.AddUnique(selectable);
        }

        public async override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            var lastSelection = _selectionManager.LastSelected;
            var nextSelection = _targets.FirstOrDefault(target => target.IsActiveInHierarchy() && (allowReselection || target != lastSelection))?.gameObject;

            if (_selectionManager.IsSelected(nextSelection) && !allowReselection)
            {
                return;
            }
            EventSystem.current.SetSelectedGameObject(nextSelection);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (updateGameObjectName)
            {
                _targets.Clear();
                _targets.AddRange(targets);
                UpdateGameObjectName();
            }
        }
#endif

        [Conditional("DEBUG")]
        private void UpdateGameObjectName()
        {
            if (_targets.IsNullOrEmpty())
            {
                return;
            }
            var stringBuilder = StringBuilderPool.Get();
            stringBuilder.Append("#router");
            foreach (var selectable in _targets)
            {
                stringBuilder.Append($"-({selectable?.name ?? "null"})");
            }

            name = StringBuilderPool.BuildAndRelease(stringBuilder);
        }

        protected override void Start()
        {
            base.Start();
            if (autoPrioritize)
            {
                foreach (var selectable in _targets)
                {
                    selectable.GetOrAddComponent<SelectedEvent>().Selected += () => { _targets.MoveElementToIndex(selectable, 0); };
                }
            }
        }
    }
}