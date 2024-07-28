using System;
using System.Collections.Generic;
using Baracuda.Utilities;
using Baracuda.Utilities.Collections;
using Unity.Burst;
using UnityEngine;

namespace Baracuda.UI
{
    /// <summary>
    ///     Represents a UI group with a unique identifier. This struct provides functionality for
    ///     creating and managing UI groups, including predefined groups such as HUD, Menu, and Overlay.
    /// </summary>
    [Serializable]
    public struct UIGroup : IEquatable<UIGroup>
    {
        #region Public API

        /// <summary>
        ///     Predefined UI group for HUD elements.
        /// </summary>
        public static readonly UIGroup HUD = Create(nameof(HUD));

        /// <summary>
        ///     Predefined UI group for Menu elements.
        /// </summary>
        public static readonly UIGroup Menu = Create(nameof(Menu));

        /// <summary>
        ///     Predefined UI group for Overlay elements.
        /// </summary>
        public static readonly UIGroup Overlay = Create(nameof(Overlay));

        /// <summary>
        ///     Creates a new UI group with a specified name.
        /// </summary>
        /// <param name="name">The name of the UI group.</param>
        /// <returns>The created UI group.</returns>
        [BurstDiscard]
        public static UIGroup Create(string name)
        {
            var value = name.ComputeFNV1aHash();
            var key = new UIGroup(value);
            Registry.AddKey(name, value);
            return key;
        }

        #endregion


        #region Fields

        [SerializeField] private int value;

        public readonly int Value => value;

        public UIGroup(int value)
        {
            this.value = value;
        }

        public bool IsValid => value != 0;

        [BurstDiscard]
        public static implicit operator UIGroup(string name)
        {
            return Create(name);
        }

        [BurstDiscard]
        public override string ToString()
        {
            return Registry.GetName(this);
        }

        #endregion


        #region IEquatable

        public bool Equals(UIGroup other)
        {
            return value == other.value;
        }

        public static bool operator ==(UIGroup rhs, UIGroup lhs)
        {
            return rhs.Equals(lhs);
        }

        public static bool operator !=(UIGroup rhs, UIGroup lhs)
        {
            return !rhs.Equals(lhs);
        }

        public override bool Equals(object obj)
        {
            return obj is UIGroup other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        #endregion


        #region Registry

        public static class Registry
        {
            public static IEnumerable<UIGroup> AllGroups()
            {
                using var buffer = new Buffer<string>(keyNames.Values);
                foreach (var name in buffer)
                {
                    yield return Create(name);
                }
            }

            public static IReadOnlyDictionary<int, string> Keys => keyNames;

            private static readonly Dictionary<int, string> keyNames = new();

            public static void AddKey(string name, in int key)
            {
                keyNames.TryAdd(key, name);
            }

            public static string GetName(in UIGroup reference)
            {
                return keyNames[reference.Value];
            }
        }

        #endregion
    }
}