using System;
using NaughtyAttributes;
using UnityEngine;

namespace Baracuda.UI
{
    [Serializable]
    public struct UIGroupSettings
    {
        [Tooltip("The starting sorting order of the UI Group.")]
        public int sortingOrder;
        [Tooltip("When enabled, the UI Group can consume escape during transitions")]
        public bool consumeEscape;
        [Tooltip("When enabled, the UI Group can consume escape during transitions")]
        public bool hasBackground;
        [ShowIf(nameof(hasBackground))]
        public UIBackground background;
    }
}