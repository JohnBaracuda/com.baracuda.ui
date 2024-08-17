using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Baracuda.UI
{
    public partial class UIGroupManager : MonoBehaviour
    {
        [PublicAPI]
        public UIBackground Background => _background;

        [PublicAPI]
        public void BlockBackground(Object source)
        {
            BlockBackgroundInternal(source);
        }

        [PublicAPI]
        public void UnblockBackground(Object source)
        {
            UnblockBackgroundInternal(source);
        }

        [PublicAPI]
        public void Block(Object source)
        {
            BlockInternal(source);
        }

        [PublicAPI]
        public void Unblock(Object source)
        {
            UnblockInternal(source);
        }

        /// <summary>
        ///     Triggered when the open sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> OpenSequenceStarted
        {
            add => _openSequenceStarted.AddListener(value);
            remove => _openSequenceStarted.RemoveListener(value);
        }

        /// <summary>
        ///     Triggered when the open sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> OpenSequenceCompleted
        {
            add => _openSequenceCompleted.AddListener(value);
            remove => _openSequenceCompleted.RemoveListener(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> CloseSequenceStarted
        {
            add => _closeSequenceStarted.AddListener(value);
            remove => _closeSequenceStarted.RemoveListener(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> CloseSequenceCompleted
        {
            add => _closeSequenceCompleted.AddListener(value);
            remove => _closeSequenceCompleted.RemoveListener(value);
        }

        /// <summary>
        ///     Triggered when a transition between two UI windows starts.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow, IWindow> TransitionStarted
        {
            add => _transitionStarted.Add(value);
            remove => _transitionStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when a transition between two UI windows completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow, IWindow> TransitionCompleted
        {
            add => _transitionCompleted.Add(value);
            remove => _transitionCompleted.Remove(value);
        }
    }
}