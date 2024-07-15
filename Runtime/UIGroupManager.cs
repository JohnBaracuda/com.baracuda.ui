using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Baracuda.UI
{
    public partial class UIGroupManager : MonoBehaviour
    {
        #region Public API

        /// <summary>
        ///     Triggered when the open sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> OpenSequenceStarted
        {
            add => _openSequenceStarted.Add(value);
            remove => _openSequenceStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the open sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> OpenSequenceCompleted
        {
            add => _openSequenceCompleted.Add(value);
            remove => _openSequenceCompleted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window starts.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> CloseSequenceStarted
        {
            add => _closeSequenceStarted.Add(value);
            remove => _closeSequenceStarted.Remove(value);
        }

        /// <summary>
        ///     Triggered when the close sequence of a UI window completes.
        /// </summary>
        [PublicAPI]
        public event Action<IWindow> CloseSequenceCompleted
        {
            add => _closeSequenceCompleted.Add(value);
            remove => _closeSequenceCompleted.Remove(value);
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

        #endregion
    }
}