using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.UI
{
    /// <summary>
    ///     The UIManager class provides a range of static methods to manage UI windows, commands, and groups.
    ///     This class offers both synchronous and asynchronous methods for various UI operations.
    ///     Most of the operations of the UIManager are wrapped in UICommands, this means that calling multiple methods like,
    ///     Open, Close, Focus etc. directly one after the other will schedule them in the order in which they are called, or
    ///     when provided with a priority value, in a prioritized order.
    /// </summary>
    public partial class UIManager : MonoBehaviour
    {
        /// <summary>
        ///     Immediately closes all open windows without playing any close animations.
        /// </summary>
        [PublicAPI]
        public static void CloseAllWindowsImmediate()
        {
            Implementation.CloseAllWindowsImmediateInternal();
        }

        /// <summary>
        ///     Asynchronously closes all open windows, with an optional mode to control the sequence of closing.
        /// </summary>
        /// <param name="closeMode">Specifies whether to close windows sequentially or concurrently.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        [PublicAPI]
        public static async UniTask CloseAllWindowsAsync(CloseMode closeMode = CloseMode.Sequential)
        {
            await Implementation.CloseAllWindowsAsyncInternal(closeMode);
        }

        /// <summary>
        ///     Executes and flushes all pending UI commands immediately.
        /// </summary>
        [PublicAPI]
        public static void FlushCommands()
        {
            Implementation.FlushCommandsInternal();
        }

        /// <summary>
        ///     Clears the queue of pending UI commands without executing them.
        /// </summary>
        [PublicAPI]
        public static void ClearCommandQueue()
        {
            Implementation.ClearCommandQueueInternal();
        }

        /// <summary>
        ///     Registers a window instance to the UI container.
        /// </summary>
        /// <param name="window">The window instance to register.</param>
        [PublicAPI]
        public static void RegisterWindow(IWindow window)
        {
            Container.Add(window.GetType(), window);
        }

        /// <summary>
        ///     Unregisters a window instance from the UI container.
        /// </summary>
        /// <param name="window">The window instance to unregister.</param>
        [PublicAPI]
        public static void UnregisterWindow(IWindow window)
        {
            Container.Remove(window.GetType(), window);
        }

        /// <summary>
        ///     Checks if any instance of a specified window type is open.
        /// </summary>
        /// <typeparam name="T">The type of the window to check.</typeparam>
        /// <returns>True if any instance of the specified window type is open; otherwise, false.</returns>
        [PublicAPI]
        public static bool IsOpen<T>() where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOpenInAnyGroup<T>();
        }

        /// <summary>
        ///     Checks if a specific window instance is open.
        /// </summary>
        /// <typeparam name="T">The type of the window to check.</typeparam>
        /// <param name="instance">The window instance to check.</param>
        /// <returns>True if the specified window instance is open; otherwise, false.</returns>
        [PublicAPI]
        public static bool IsOpen<T>(T instance) where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOpenInAnyGroup(instance);
        }

        /// <summary>
        ///     Checks if any instance of a specified window type is closed.
        /// </summary>
        /// <typeparam name="T">The type of the window to check.</typeparam>
        /// <returns>True if any instance of the specified window type is closed; otherwise, false.</returns>
        [PublicAPI]
        public static bool IsClosed<T>() where T : MonoBehaviour, IWindow
        {
            return !Implementation.IsOpenInAnyGroup<T>();
        }

        /// <summary>
        ///     Checks if a specific window instance is closed.
        /// </summary>
        /// <typeparam name="T">The type of the window to check.</typeparam>
        /// <param name="instance">The window instance to check.</param>
        /// <returns>True if the specified window instance is closed; otherwise, false.</returns>
        [PublicAPI]
        public static bool IsClosed<T>(T instance) where T : MonoBehaviour, IWindow
        {
            return !Implementation.IsOpenInAnyGroup(instance);
        }

        /// <summary>
        ///     Retrieves an instance of a specified window type from the container.
        /// </summary>
        /// <typeparam name="T">The type of the window to retrieve.</typeparam>
        /// <returns>The window instance of the specified type.</returns>
        [PublicAPI]
        public static T GetWindow<T>() where T : MonoBehaviour, IWindow
        {
            return Container.Load<T>();
        }

        /// <summary>
        ///     Executes a UI command asynchronously.
        /// </summary>
        /// <param name="command">The UI command to execute.</param>
        [PublicAPI]
        public static void ExecuteCommand(UICommand command)
        {
            Implementation.ProcessCommandInternal(command).Forget();
        }

        /// <summary>
        ///     Executes a UI command asynchronously and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the result window.</typeparam>
        /// <param name="command">The UI command to execute.</param>
        /// <returns>A UniTask representing the asynchronous operation, with the result window.</returns>
        [PublicAPI]
        public static async UniTask<T> ExecuteCommandAsync<T>(UICommand command) where T : MonoBehaviour, IWindow
        {
            return (T)await Implementation.ProcessCommandInternal(command);
        }

        /// <summary>
        ///     Executes a UI command asynchronously and returns the result window.
        /// </summary>
        /// <param name="command">The UI command to execute.</param>
        /// <returns>A UniTask representing the asynchronous operation, with the result window.</returns>
        [PublicAPI]
        public static async UniTask<IWindow> ExecuteCommandAsync(UICommand command)
        {
            return await Implementation.ProcessCommandInternal(command);
        }

        /// <summary>
        ///     Opens a specified window instance.
        /// </summary>
        /// <typeparam name="T">The type of the window to open.</typeparam>
        /// <param name="instance">The window instance to open. If null, a new instance is created.</param>
        [PublicAPI]
        public static void Open<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Open().WithInstance(instance).Execute();
        }

        /// <summary>
        ///     Closes a specified window instance.
        /// </summary>
        /// <typeparam name="T">The type of the window to close.</typeparam>
        /// <param name="instance">The window instance to close. If null, the first instance found is closed.</param>
        [PublicAPI]
        public static void Close<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Close().WithInstance(instance).Execute();
        }

        /// <summary>
        ///     Toggles the open/closed state of a specified window instance.
        /// </summary>
        /// <typeparam name="T">The type of the window to toggle.</typeparam>
        /// <param name="instance">The window instance to toggle. If null, the first instance found is toggled.</param>
        [PublicAPI]
        public static void Toggle<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Toggle().WithInstance(instance).Execute();
        }

        /// <summary>
        ///     Loads a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to load.</typeparam>
        [PublicAPI]
        public static void Load<T>() where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Load().Execute();
        }

        /// <summary>
        ///     Unloads a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to unload.</typeparam>
        [PublicAPI]
        public static void Unload<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Unload().WithInstance(instance).Execute();
        }

        /// <summary>
        ///     Creates a command to open a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to open.</typeparam>
        /// <returns>A UICommand to open the specified window type.</returns>
        [PublicAPI]
        public static UICommand<T> OpenCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Open();
        }

        /// <summary>
        ///     Creates a command to close a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to close.</typeparam>
        /// <returns>A UICommand to close the specified window type.</returns>
        [PublicAPI]
        public static UICommand<T> CloseCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Close();
        }

        /// <summary>
        ///     Creates a command to toggle the open/closed state of a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to toggle.</typeparam>
        /// <returns>A UICommand to toggle the specified window type.</returns>
        [PublicAPI]
        public static UICommand<T> ToggleCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Toggle();
        }

        /// <summary>
        ///     Creates a command to load a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to load.</typeparam>
        /// <returns>A UICommand to load the specified window type.</returns>
        [PublicAPI]
        public static UICommand<T> LoadCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Load();
        }

        /// <summary>
        ///     Creates a command to unload a specified window type.
        /// </summary>
        /// <typeparam name="T">The type of the window to unload.</typeparam>
        /// <returns>A UICommand to unload the specified window type.</returns>
        [PublicAPI]
        public static UICommand<T> UnloadCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Unload();
        }

        /// <summary>
        ///     Gets the UIGroupManager for a specified UIGroupReference.
        /// </summary>
        /// <param name="group">The UIGroupReference for which to get the manager.</param>
        /// <returns>The UIGroupManager for the specified group reference.</returns>
        [PublicAPI]
        public static UIGroupManager GetGroupManagerFor(UIGroup group)
        {
            return Implementation._groups[group];
        }

        /// <summary>
        ///     Gets the UIGroupManager for the HUD group.
        /// </summary>
        [PublicAPI]
        public static UIGroupManager HUDGroup => GetGroupManagerFor(UIGroup.HUD);

        /// <summary>
        ///     Gets the UIGroupManager for the Menu group.
        /// </summary>
        [PublicAPI]
        public static UIGroupManager MenuGroup => GetGroupManagerFor(UIGroup.Menu);

        /// <summary>
        ///     Gets the UIGroupManager for the Overlay group.
        /// </summary>
        [PublicAPI]
        public static UIGroupManager OverlayGroup => GetGroupManagerFor(UIGroup.Overlay);

        /// <summary>
        ///     Gets the UIContainer instance.
        /// </summary>
        [PublicAPI]
        public static UIContainer Container => Implementation._container;
    }
}