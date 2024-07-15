using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.UI
{
    public class UICommand
    {
        #region Static API

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Open<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Open,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Close<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Close,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Load<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Load,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Unload<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Unload,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Toggle<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Toggle,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand Focus<T>() where T : MonoBehaviour, IWindow
        {
            var command = new UICommand
            {
                CommandType = UICommandType.Focus,
                WindowType = typeof(T)
            };
            return command;
        }

        #endregion


        #region Data

        public IWindow Window { get; private protected set; }
        public Type WindowType { get; internal set; }
        public UICommandType CommandType { get; internal set; }
        public List<Type> AboveTypes { get; } = new();
        public List<Type> BellowTypes { get; } = new();
        public bool SkipAnimation { get; private protected set; }
        public bool ExecuteImmediate { get; private protected set; }
        public int Priority { get; private protected set; }
        public UIGroupReference Group { get; private protected set; }

        #endregion


        #region Execution API

        [PublicAPI]
        public void Execute()
        {
            UIManager.ExecuteCommand(this);
        }

        [PublicAPI]
        public UniTask<T> ExecuteAsync<T>() where T : MonoBehaviour, IWindow
        {
            return UIManager.ExecuteCommandAsync<T>(this);
        }

        [PublicAPI]
        public UniTask<IWindow> ExecuteAsync()
        {
            return UIManager.ExecuteCommandAsync(this);
        }

        #endregion


        #region Builder API

        /*
         * We can add methods like AsNewInstance() and ForAllInstances()
         * to manage multiple instances of the same type from this API.
         * Adding additional methods like WithParallelExecution would also
         * allow us to play open and close animations in parallel if needed.
         * We should add support for parallel closing to the UIManager API.
         */

        /// <summary>
        ///     Set the instance that is used instead of the one registered to the UI system.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand WithInstance(IWindow instance)
        {
            Window = instance;
            return this;
        }

        /// <summary>
        ///     Complete ongoing transitions, skip queued commands and execute this command immediate.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand WithImmediateExecution()
        {
            ExecuteImmediate = true;
            return this;
        }

        /// <summary>
        ///     Skip fade in and fade out animations for the window.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand WithoutAnimation()
        {
            SkipAnimation = true;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand WithGroup(UIGroupReference groupReference)
        {
            Group = groupReference;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to HUD.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand InHUDGroup()
        {
            Group = UIGroupReference.HUD;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to Menu.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand InMenuGroup()
        {
            Group = UIGroupReference.Menu;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to Overlay.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand InOverlayGroup()
        {
            Group = UIGroupReference.Overlay;
            return this;
        }

        /// <summary>
        ///     Set the priority to determine the order when a scheduled ui command is executed
        ///     compared to other scheduled commands
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        /// <summary>
        ///     Set the rendering sorting order of the window to be higher than this.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand OrderAbove<TAbove>() where TAbove : MonoBehaviour, IWindow
        {
            AboveTypes.Add(typeof(TAbove));
            return this;
        }

        /// <summary>
        ///     Set the rendering sorting order of the window to be lower than this.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public UICommand OrderBelow<TBelow>() where TBelow : MonoBehaviour, IWindow
        {
            BellowTypes.Add(typeof(TBelow));
            return this;
        }

        #endregion
    }
}