using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.UI
{
    /// <summary>
    ///     Generic wrapper class for a <see cref="UICommand" /> adds an additional layer of type safety and generic execution.
    /// </summary>
    public sealed class UICommand<T> : UICommand where T : MonoBehaviour, IWindow
    {
        #region Static API

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Open()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Open,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Close()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Close,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Load()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Load,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Unload()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Unload,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Toggle()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Toggle,
                WindowType = typeof(T)
            };
            return command;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static UICommand<T> Focus()
        {
            var command = new UICommand<T>
            {
                CommandType = UICommandType.Focus,
                WindowType = typeof(T)
            };
            return command;
        }

        #endregion


        #region Execution API

        [PublicAPI]
        public new UniTask<T> ExecuteAsync()
        {
            return UIManager.ExecuteCommandAsync<T>(this);
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
        public new UICommand<T> WithInstance(IWindow instance)
        {
            Window = instance;
            return this;
        }

        /// <summary>
        ///     Complete ongoing transitions, skip queued commands and execute this command immediate.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> WithImmediateExecution()
        {
            ExecuteImmediate = true;
            return this;
        }

        /// <summary>
        ///     Skip fade in and fade out animations for the window.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> WithoutAnimation()
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
        public new UICommand<T> WithGroup(UIGroup group)
        {
            Group = group;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to HUD.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> InHUDGroup()
        {
            Group = UIGroup.HUD;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to Menu.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> InMenuGroup()
        {
            Group = UIGroup.Menu;
            return this;
        }

        /// <summary>
        ///     Set the ui group to which the window is assigned to, to Overlay.
        ///     Groups handle transitions individually and are displayed on different layers.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> InOverlayGroup()
        {
            Group = UIGroup.Overlay;
            return this;
        }

        /// <summary>
        ///     Set the priority to determine the order when a scheduled ui command is executed
        ///     compared to other scheduled commands
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        /// <summary>
        ///     Set the rendering sorting order of the window to be higher than this.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> OrderAbove<TAbove>() where TAbove : MonoBehaviour, IWindow
        {
            AboveTypes.Add(typeof(TAbove));
            return this;
        }

        /// <summary>
        ///     Set the rendering sorting order of the window to be lower than this.
        /// </summary>
        [PublicAPI]
        [MustUseReturnValue]
        public new UICommand<T> OrderBelow<TBelow>() where TBelow : MonoBehaviour, IWindow
        {
            BellowTypes.Add(typeof(TBelow));
            return this;
        }

        #endregion
    }
}