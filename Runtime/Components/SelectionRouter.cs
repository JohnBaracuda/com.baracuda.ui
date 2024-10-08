﻿using System.Linq;
using Baracuda.UI.Selection;
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
        [SerializeField] private Selectable[] targets;

        public async override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            var lastSelection = ServiceLocator.Get<SelectionManager>().LastSelected;
            var nextSelection = targets
                .FirstOrDefault(target => target.IsActiveInHierarchy() && target != lastSelection)?.gameObject;

            EventSystem.current.SetSelectedGameObject(nextSelection);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            var stringBuilder = StringBuilderPool.Get();
            stringBuilder.Append("#router");
            foreach (var selectable in targets)
            {
                stringBuilder.Append($"-({selectable?.name ?? "null"})");
            }

            name = StringBuilderPool.BuildAndRelease(stringBuilder);
        }
#endif
    }
}