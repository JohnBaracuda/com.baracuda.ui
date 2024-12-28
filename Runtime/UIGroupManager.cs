using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Baracuda.UI
{
    public partial class UIGroupManager : MonoBehaviour
    {
        [PublicAPI]
        public UIBackground Background { get; private set; }

        [PublicAPI]
        public void BlockBackground(object source)
        {
            BlockBackgroundInternal(source);
        }

        [PublicAPI]
        public void UnblockBackground(object source)
        {
            UnblockBackgroundInternal(source);
        }

        [PublicAPI]
        public void Block(object source)
        {
            BlockInternal(source);
        }

        [PublicAPI]
        public void Unblock(object source)
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
            add => _transitionStarted.AddListener(value);
            remove => _transitionStarted.RemoveListener(value);
        }

        /// <summary>
        ///     Triggered when a transition between two UI windows completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow, IWindow> TransitionCompleted
        {
            add => _transitionCompleted.AddListener(value);
            remove => _transitionCompleted.RemoveListener(value);
        }
    }
}