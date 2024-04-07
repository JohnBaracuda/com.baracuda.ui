using Baracuda.Mediator.Callbacks;
using Baracuda.Mediator.Singleton;
using Baracuda.Tools;
using Baracuda.Utilities.Types;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace Baracuda.UI
{
    public class UISystem : SingletonAsset<UISystem>
    {
        #region Inspector

        [SerializeField] [Required] private InputActionReference returnInput;
        private readonly StackList<IReturnConsumer> _returnConsumerStack = new();
        private readonly StackList<UIComponent> _uiStack = new();

        private int _returnPressedLocks;

        [ReadonlyInspector]
        private List<UIComponent> DebugUI => _uiStack.ToList();

        [ReadonlyInspector]
        private List<IReturnConsumer> DebugReturn => _returnConsumerStack.ToList();

        #endregion


        #region Initialization & Shutdown

        [CallbackOnInitialization]
        private void Initialize()
        {
            returnInput.action.performed -= OnReturnPressed;
            returnInput.action.performed += OnReturnPressed;
        }

        [CallbackOnApplicationQuit]
        private void Shutdown()
        {
            _returnConsumerStack.Clear();
            _uiStack.Clear();
            _returnPressedLocks = 0;
            returnInput.action.performed -= OnReturnPressed;
        }

        #endregion


        #region Public API

        public void CloseAllWindowsImmediate()
        {
            while (_uiStack.Any())
            {
                _uiStack.Peek().CloseImmediate();
            }
        }

        public async UniTask CloseAllWindowsAsync()
        {
            var buffer = new Stack<UIComponent>(_uiStack);
            _uiStack.Clear();
            while (buffer.TryPop(out var element))
            {
                if (element.IsVisible)
                {
                    Debug.Log("UI System", $"Closing Async {element}");
                    await element.CloseAsync();
                    Debug.Log("UI System", $"Closed Async {element}");
                }
                else
                {
                    Debug.Log("UI System", $"Closing Immediate {element}");
                    element.CloseImmediate();
                }
            }
        }

        public StackList<IReturnConsumer> ReturnConsumerStack => _returnConsumerStack;
        public StackList<UIComponent> UIStack => _uiStack;

        #endregion


        #region Return Pressed

        private void OnReturnPressed(InputAction.CallbackContext context)
        {
            if (_returnPressedLocks > 0)
            {
                Debug.Log("UI System", "Blocked Return Input");
                return;
            }
            if (UIComponent.TransitionSequence.isAlive)
            {
                return;
            }
            foreach (var consumer in _returnConsumerStack.Reverse())
            {
                if (consumer.TryConsumeReturn())
                {
                    break;
                }
            }
        }

        public void LockReturnConsume()
        {
            _returnPressedLocks++;
        }

        public async void UnlockReturnConsume()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            _returnPressedLocks--;
        }

        #endregion
    }
}