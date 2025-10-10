using Adventure.Domain.Movement;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.Scripts.Adventure.Infrastructure.Input;

namespace Adventure.Infrastructure.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private MovementSettingsSO settings;
        [SerializeField] private Transform cameraPivot;

        private CharacterController _controller;
        private PlayerMovementService _movementService;
        private PlayerMovementState _state;
        private float _yaw;
        private Vector2 _moveInput;
        private bool _sprintInput;
        private Vector2 _pendingLookInput;
        private bool _useExternalInput;
        [FormerlySerializedAs("playerInput")]
        [SerializeField] private AdventurePlayerInput legacyInputProvider;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("MovementSettingsSO not assigned", this);
                enabled = false;
                return;
            }
            _controller = GetComponent<CharacterController>();
            _movementService = new PlayerMovementService(settings.ToDomain());
            _state = new PlayerMovementState(Vector3.zero, _controller.isGrounded);
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            Vector2 move;
            Vector2 lookDelta;
            bool sprint;

            if (_useExternalInput)
            {
                move = _moveInput;
                lookDelta = _pendingLookInput;
                sprint = _sprintInput;
                _pendingLookInput = Vector2.zero;
            }
            else
            {
                var provider = legacyInputProvider != null ? legacyInputProvider : GetComponent<AdventurePlayerInput>();
                if (provider != null)
                {
                    move = provider.Move;
                    lookDelta = provider.Look;
                    sprint = provider.IsSprinting;
                }
                else
                {
                    move = Vector2.zero;
                    lookDelta = Vector2.zero;
                    sprint = false;
                }
            }

            var movementInput = new MovementInput(move, lookDelta, sprint);

            _yaw += movementInput.Look.x * settings.LookSensitivity;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            var command = _movementService.Tick(_state, movementInput, transform.rotation, deltaTime);
            UpdateCameraPitch(command.DesiredPitch);

            PerformMove(command.Velocity, deltaTime);
        }

        public void SetMoveInput(Vector2 move)
        {
            _useExternalInput = true;
            _moveInput = move;
        }

        public void SetSprintInput(bool sprint)
        {
            _useExternalInput = true;
            _sprintInput = sprint;
        }

        public void EnqueueLookDelta(Vector2 delta)
        {
            _useExternalInput = true;
            _pendingLookInput += delta;
        }

        public void ResetExternalInput()
        {
            _useExternalInput = false;
            _moveInput = Vector2.zero;
            _sprintInput = false;
            _pendingLookInput = Vector2.zero;
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
    }
}
