using System;
using System.Collections.Generic;
using Baracuda.Utilities;
using Baracuda.Utilities.Collections;
using Unity.Burst;
using UnityEngine;

namespace Baracuda.UI
{
    [Serializable]
    public struct UIGroupReference : IEquatable<UIGroupReference>
    {
        #region Fundamentals

        public static readonly UIGroupReference HUD = Create(nameof(HUD));
        public static readonly UIGroupReference Menu = Create(nameof(Menu));
        public static readonly UIGroupReference Overlay = Create(nameof(Overlay));

        #endregion


        #region Fields

        [SerializeField] private int value;

        public readonly int Value => value;

        public UIGroupReference(int value)
        {
            this.value = value;
        }

        public bool IsValid => value != 0;

        [BurstDiscard]
        public static UIGroupReference Create(string name)
        {
            var value = name.ComputeFNV1aHash();
            var key = new UIGroupReference(value);
            Registry.AddKey(name, value);
            return key;
        }

        [BurstDiscard]
        public static implicit operator UIGroupReference(string name)
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

        public bool Equals(UIGroupReference other)
        {
            return value == other.value;
        }

        public static bool operator ==(UIGroupReference rhs, UIGroupReference lhs)
        {
            return rhs.Equals(lhs);
        }

        public static bool operator !=(UIGroupReference rhs, UIGroupReference lhs)
        {
            return !rhs.Equals(lhs);
        }

        public override bool Equals(object obj)
        {
            return obj is UIGroupReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        #endregion


        #region Registry

        public static class Registry
        {
            public static IEnumerable<UIGroupReference> AllGroups()
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

            public static string GetName(in UIGroupReference reference)
            {
                return keyNames[reference.Value];
            }
        }

        #endregion
    }
}