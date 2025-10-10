using System;
using Adventure.Domain.Movement;
using Adventure.Infrastructure.Input;
using UnityEngine;

namespace Adventure.Infrastructure.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour, IDisposable
    {
        [SerializeField] private MovementSettingsSO settings;
        [SerializeField] private Transform cameraPivot;

        private CharacterController _controller;
        private AdventureInputReader _inputReader;
        private PlayerMovementService _movementService;
        private PlayerMovementState _state;
        private float _yaw;

        public event Action Interact;
        public event Action Collect;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("MovementSettingsSO not assigned", this);
                enabled = false;
                return;
            }
            _controller = GetComponent<CharacterController>();
            _inputReader = new AdventureInputReader();
            _movementService = new PlayerMovementService(settings.ToDomain());
            _state = new PlayerMovementState(Vector3.zero, _controller.isGrounded);
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var input = _inputReader.Read();

            _yaw += input.Look.x * settings.LookSensitivity;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            var command = _movementService.Tick(_state, input, deltaTime);
            UpdateCameraPitch(command.DesiredPitch);

            var desiredVelocity = command.DesiredVelocity;
            var worldHorizontal = transform.TransformDirection(new Vector3(desiredVelocity.x, 0f, desiredVelocity.z));
            var worldVelocity = new Vector3(worldHorizontal.x, desiredVelocity.y, worldHorizontal.z);
            PerformMove(worldVelocity, deltaTime);

            if (command.InteractRequested)
                Interact?.Invoke();
            if (command.CollectRequested)
                Collect?.Invoke();
        }

        private void PerformMove(Vector3 velocity, float deltaTime)
        {
            var displacement = velocity * deltaTime;
            var collision = _controller.Move(displacement);
            if ((collision & CollisionFlags.Sides) != 0 && settings.StepHeight > 0f)
            {
                TryStepUp(displacement);
            }

            _state.SetGrounded(_controller.isGrounded);
        }

        private void TryStepUp(Vector3 attemptedMove)
        {
            var origin = transform.position + Vector3.up * (settings.StepHeight + 0.05f);
            var direction = new Vector3(attemptedMove.x, 0f, attemptedMove.z).normalized;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            if (!Physics.Raycast(origin, direction, _controller.radius + 0.1f, settings.CollisionMask))
            {
                var verticalOffset = Vector3.up * settings.StepHeight;
                _controller.Move(verticalOffset);
                _controller.Move(new Vector3(attemptedMove.x, 0f, attemptedMove.z));
            }
        }

        private void UpdateCameraPitch(float pitch)
        {
            if (cameraPivot == null)
                return;

            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void Dispose()
        {
            _inputReader?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
