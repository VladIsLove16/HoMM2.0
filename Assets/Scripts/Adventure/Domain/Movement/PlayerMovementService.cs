using UnityEngine;

namespace Adventure.Domain.Movement
{
    /// <summary>
    /// Pure domain service responsible for translating input into motion intents.
    /// No knowledge about Unity components or physics implementation details.
    /// </summary>
    public sealed class PlayerMovementService
    {
        private readonly MovementSettings _settings;

        public PlayerMovementService(MovementSettings settings)
        {
            _settings = settings;
        }

        public MovementCommand Tick(PlayerMovementState state, MovementInput input, Quaternion orientation, float deltaTime)
        {
            var targetSpeed = input.Sprint ? _settings.MoveSpeed * _settings.SprintMultiplier : _settings.MoveSpeed;
            var moveDir = new Vector3(input.Move.x, 0f, input.Move.y);
            moveDir = Vector3.ClampMagnitude(moveDir, 1f);

            var worldMoveDir = orientation * moveDir;
            worldMoveDir.y = 0f;
            worldMoveDir = Vector3.ClampMagnitude(worldMoveDir, 1f);

            var desiredVelocity = worldMoveDir * targetSpeed;

            // accelerate towards desired velocity
            var velocity = state.Velocity;
            var horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            var diff = desiredVelocity - horizontalVelocity;
            var accel = _settings.Acceleration * deltaTime;
            var newHorizontalVelocity = horizontalVelocity + Vector3.ClampMagnitude(diff, accel);

            var vertical = velocity.y;
            if (state.IsGrounded && velocity.y < 0f)
            {
                vertical = -_settings.GroundCheckDistance; // small downward force to keep grounded
            }
            vertical += _settings.Gravity * deltaTime;

            var finalVelocity = new Vector3(newHorizontalVelocity.x, vertical, newHorizontalVelocity.z);
            state.SetVelocity(finalVelocity);

            // camera pitch integration
            var pitch = state.Pitch - input.Look.y * _settings.LookSensitivity;
            pitch = Mathf.Clamp(pitch, -_settings.MaxLookPitch, _settings.MaxLookPitch);
            state.SetPitch(pitch);

            return new MovementCommand(finalVelocity, pitch);
        }
    }
}
