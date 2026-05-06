using Adventure.Domain.Movement;
using Adventure.Settings.Configuration;
using SharedView.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.Scripts.Adventure.Infrastructure.Input;
using Zenject;
using System;

namespace Adventure.Infrastructure.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        private const float VELOCITYTHRESHOLD = 0.0001f;
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
        private float _nextFootstepTime;
        [FormerlySerializedAs("adventureCharacterInput")]
        [Inject(Optional = true)] private AdventureInput legacyInputProvider;
        [Inject(Optional = true)] private IMouseSensitivityService _mouseSensitivityService;
        [Inject(Optional = true)] private IGameAudioService _audioService;

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
                var provider = legacyInputProvider;
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

            var lookMultiplier = ResolveLookMultiplier();
            var scaledLook = lookDelta * lookMultiplier;

            var movementInput = new MovementInput(move, scaledLook, sprint);

            _yaw += movementInput.Look.x * settings.LookSensitivity;

            var command = _movementService.Tick(_state, movementInput, transform.rotation, deltaTime);
            Rotate(command);

            PerformMove(command.Velocity, deltaTime);
        }

        private void Rotate(MovementCommand command)
        {
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraPivot == null)
                return;
            cameraPivot.localRotation = Quaternion.Euler(command.DesiredPitch, 0f, 0f);
        }

        private float ResolveLookMultiplier()
        {
            return _mouseSensitivityService?.RotationMultiplier ?? 1f;
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
            if(velocity.sqrMagnitude < VELOCITYTHRESHOLD)
            {
                return;
            }
            var displacement = velocity * deltaTime;
            var collision = _controller.Move(displacement);
            if ((collision & CollisionFlags.Sides) != 0 && settings.StepHeight > 0f)
            {
                TryStepUp(displacement);
            }

            _state.SetGrounded(_controller.isGrounded);
            TryPlayFootstep(velocity);
        }

        private void TryPlayFootstep(Vector3 velocity)
        {
            if (_audioService == null || !_controller.isGrounded)
                return;

            var horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.sqrMagnitude < 0.04f)
                return;

            if (Time.time < _nextFootstepTime)
                return;

            var settingsSo = _audioService.Settings;
            var interval = _sprintInput
                ? settingsSo != null ? settingsSo.SprintStepInterval : 0.34f
                : settingsSo != null ? settingsSo.WalkStepInterval : 0.48f;

            _nextFootstepTime = Time.time + Mathf.Max(0.05f, interval);
            _audioService.PlayPlayerFootstep(transform.position);
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


        public Quaternion BodyRotation => transform.rotation;

        public Quaternion CameraLocalRotation => cameraPivot != null
            ? cameraPivot.localRotation
            : Quaternion.identity;

        public void RotateTowards(Vector3 worldPosition)
        {
            var bodyDirection = worldPosition - transform.position;
            bodyDirection.y = 0f;
            if (bodyDirection.sqrMagnitude > Mathf.Epsilon)
            {
                var bodyRotation = Quaternion.LookRotation(bodyDirection.normalized, Vector3.up);
                ApplyBodyRotation(bodyRotation);
            }

            if (cameraPivot == null)
                return;

            var cameraDirection = worldPosition - cameraPivot.position;
            if (cameraDirection.sqrMagnitude <= Mathf.Epsilon)
                return;

            var flat = new Vector3(cameraDirection.x, 0f, cameraDirection.z);
            var pitch = flat.sqrMagnitude > Mathf.Epsilon
                ? -Mathf.Atan2(cameraDirection.y, flat.magnitude) * Mathf.Rad2Deg
                : 0f;
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void RestoreOrientation(Quaternion bodyRotation, Quaternion cameraRotation)
        {
            ApplyBodyRotation(bodyRotation);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = cameraRotation;
            }
        }

        private void ApplyBodyRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            _yaw = rotation.eulerAngles.y;
        }
    }
}
