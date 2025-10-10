using Adventure.Domain.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Adventure.Infrastructure.Input
{
    public sealed class AdventureInputReader : System.IDisposable
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _sprintAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _collectAction;

        public AdventureInputReader()
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            var wasd = _moveAction.AddCompositeBinding("2DVector");
            wasd.With("up", "<Keyboard>/w");
            wasd.With("down", "<Keyboard>/s");
            wasd.With("left", "<Keyboard>/a");
            wasd.With("right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            _lookAction = new InputAction("Look", InputActionType.Value, expectedControlType: "Vector2");
            _lookAction.AddBinding("<Mouse>/delta");
            _lookAction.AddBinding("<Gamepad>/rightStick");

            _sprintAction = new InputAction("Sprint", InputActionType.Button);
            _sprintAction.AddBinding("<Keyboard>/leftShift");
            _sprintAction.AddBinding("<Gamepad>/leftStickPress");

            _interactAction = new InputAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e");
            _interactAction.AddBinding("<Gamepad>/buttonSouth");

            _collectAction = new InputAction("Collect", InputActionType.Button);
            _collectAction.AddBinding("<Mouse>/leftButton");
            _collectAction.AddBinding("<Gamepad>/rightTrigger");

            _moveAction.Enable();
            _lookAction.Enable();
            _sprintAction.Enable();
            _interactAction.Enable();
            _collectAction.Enable();
        }

        public MovementInput Read()
        {
            var move = _moveAction.ReadValue<Vector2>();
            var look = _lookAction.ReadValue<Vector2>();
            var sprint = _sprintAction.IsPressed();
            var interact = _interactAction.WasPressedThisFrame();
            var collect = _collectAction.WasPressedThisFrame();
            return new MovementInput(move, look, sprint, interact, collect);
        }

        public void Dispose()
        {
            _moveAction?.Dispose();
            _lookAction?.Dispose();
            _sprintAction?.Dispose();
            _interactAction?.Dispose();
            _collectAction?.Dispose();
        }
    }
}
