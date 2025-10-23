using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Assets.Scripts.Adventure.Infrastructure.Input
{
    [DefaultExecutionOrder(-200)]
    public sealed class AdventureInput :  IDisposable
    {
        private InputSystem_Adventure _playerMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _interactAction;
        private InputAction _showHintAction;
        private InputAction _openSettingsAction;
        private InputAction _openBookAction;

        public event Action<Vector2> MoveChanged;
        public event Action<Vector2> LookChanged;
        public event Action<bool> SprintChanged;
        public event Action InteractPerformed;
        public event Action ShowHintPerformed;
        public event Action OpenSettingsPerformed;
        public event Action OpenMushroomBookPerformed;

        public Vector2 Move => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsSprinting => _sprintAction != null && _sprintAction.IsPressed();
        [Inject]
        private void Construct()
        {
            _playerMap = new InputSystem_Adventure();
            _playerMap.Enable();
            _moveAction = _playerMap.Character.Move;
            _lookAction = _playerMap.Character.Look;
            _sprintAction = _playerMap.Character.Sprint;
            _interactAction = _playerMap.Character.Interact;
            _showHintAction = _playerMap.Character.ShowHint;
            _openSettingsAction = _playerMap.Menus.OpenSettings;
            _openBookAction = _playerMap.Menus.OpenMushroomBook;
            SubscribeActionCallbacks();
        }

        private void OnDisable()
        {
            if (_playerMap == null)
                return;

            _playerMap.Disable();

            // ensure consumers reset state when input turns off
            MoveChanged?.Invoke(Vector2.zero);
            SprintChanged?.Invoke(false);
        }

        private void SubscribeActionCallbacks()
        {
            Debug.Log("SubscribeActionCallbacks");
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed += OnLookPerformed;
                _lookAction.canceled += OnLookCanceled;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprintPerformed;
                _sprintAction.canceled += OnSprintCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
            }

            if (_showHintAction != null)
            {
                _showHintAction.performed += OnCollectPerformed;
            }

            if (_openSettingsAction != null)
            {
                _openSettingsAction.performed += OnOpenSettingsPerformed;
            }

            if (_openBookAction != null)
            {
                _openBookAction.performed += OnOpenBookPerformed;
            }
        }

        private void UnsubscribeActionCallbacks()
        {
            _playerMap.Disable();
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_lookAction != null)
            {
                _lookAction.performed -= OnLookPerformed;
                _lookAction.canceled -= OnLookCanceled;
            }

            if (_sprintAction != null)
            {
                _sprintAction.performed -= OnSprintPerformed;
                _sprintAction.canceled -= OnSprintCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
            }

            if (_showHintAction != null)
            {
                _showHintAction.performed -= OnCollectPerformed;
            }

            if (_openSettingsAction != null)
            {
                _openSettingsAction.performed -= OnOpenSettingsPerformed;
            }

            if (_openBookAction != null)
            {
                _openBookAction.performed -= OnOpenBookPerformed;
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Debug.Log("OnMovePerformed ");
            MoveChanged?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MoveChanged?.Invoke(Vector2.zero);
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            LookChanged?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnLookCanceled(InputAction.CallbackContext _)
        {
            LookChanged?.Invoke(Vector2.zero);
        }

        private void OnSprintPerformed(InputAction.CallbackContext _)
        {
            SprintChanged?.Invoke(true);
        }

        private void OnSprintCanceled(InputAction.CallbackContext _)
        {
            SprintChanged?.Invoke(false);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("InteractPerformed?.Invoke();");
                InteractPerformed?.Invoke();
            }
        }

        private void OnCollectPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
                ShowHintPerformed?.Invoke();
        }

        private void OnOpenSettingsPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
                OpenSettingsPerformed?.Invoke();
        }

        private void OnOpenBookPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OpenMushroomBookPerformed?.Invoke();
            }
        }

        public void Dispose()
        {
            UnsubscribeActionCallbacks();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
