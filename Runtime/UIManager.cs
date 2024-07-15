using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.UI
{
    public partial class UIManager : MonoBehaviour
    {
        [PublicAPI]
        public static void CloseAllWindowsImmediate()
        {
            Implementation.CloseAllWindowsImmediateInternal();
        }

        [PublicAPI]
        public static async UniTask CloseAllWindowsAsync(CloseMode closeMode = CloseMode.Sequential)
        {
            await Implementation.CloseAllWindowsAsyncInternal(closeMode);
        }

        [PublicAPI]
        public static void FlushCommands()
        {
            Implementation.FlushCommandsInternal();
        }

        [PublicAPI]
        public static void ClearCommandQueue()
        {
            Implementation.ClearCommandQueueInternal();
        }

        [PublicAPI]
        public static void RegisterWindow(IWindow window)
        {
            Container.Add(window.GetType(), window);
        }

        [PublicAPI]
        public static void UnregisterWindow(IWindow window)
        {
            Container.Remove(window.GetType(), window);
        }

        [PublicAPI]
        public static bool IsOpen<T>() where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOpenInAnyGroup<T>();
        }

        [PublicAPI]
        public static bool IsOpen<T>(T instance) where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOpenInAnyGroup(instance);
        }

        [PublicAPI]
        public static bool IsOrWillOpen<T>() where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOrWillOpenInAnyGroup<T>();
        }

        [PublicAPI]
        public static bool IsOrWillOpen<T>(T instance) where T : MonoBehaviour, IWindow
        {
            return Implementation.IsOrWillOpenInAnyGroup(instance);
        }

        [PublicAPI]
        public static T GetWindow<T>() where T : MonoBehaviour, IWindow
        {
            return Container.Load<T>();
        }

        [PublicAPI]
        public static void ExecuteCommand(UICommand command)
        {
            Implementation.ProcessCommandInternal(command).Forget();
        }

        [PublicAPI]
        public static async UniTask<T> ExecuteCommandAsync<T>(UICommand command) where T : MonoBehaviour, IWindow
        {
            return (T)await Implementation.ProcessCommandInternal(command);
        }

        [PublicAPI]
        public static async UniTask<IWindow> ExecuteCommandAsync(UICommand command)
        {
            return await Implementation.ProcessCommandInternal(command);
        }

        [PublicAPI]
        public static void Open<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Open().WithInstance(instance).Execute();
        }

        [PublicAPI]
        public static void Close<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Close().WithInstance(instance).Execute();
        }

        [PublicAPI]
        public static void Toggle<T>(T instance = null) where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Toggle().WithInstance(instance).Execute();
        }

        [PublicAPI]
        public static void Load<T>() where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Load().Execute();
        }

        [PublicAPI]
        public static void Unload<T>() where T : MonoBehaviour, IWindow
        {
            UICommand<T>.Unload().Execute();
        }

        [PublicAPI]
        public static UICommand<T> OpenCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Open();
        }

        [PublicAPI]
        public static UICommand<T> CloseCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Close();
        }

        [PublicAPI]
        public static UICommand<T> ToggleCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Toggle();
        }

        [PublicAPI]
        public static UICommand<T> LoadCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Load();
        }

        [PublicAPI]
        public static UICommand<T> UnloadCommand<T>() where T : MonoBehaviour, IWindow
        {
            return UICommand<T>.Unload();
        }

        [PublicAPI]
        public static UIGroupManager GetGroupManagerFor(UIGroupReference groupReference)
        {
            return Implementation._groups[groupReference];
        }

        [PublicAPI]
        public static UIGroupManager HUDGroup => GetGroupManagerFor(UIGroupReference.HUD);

        [PublicAPI]
        public static UIGroupManager MenuGroup => GetGroupManagerFor(UIGroupReference.Menu);

        [PublicAPI]
        public static UIGroupManager OverlayGroup => GetGroupManagerFor(UIGroupReference.Overlay);

        [PublicAPI]
        public static UIContainer Container => Implementation._container;
    }
}