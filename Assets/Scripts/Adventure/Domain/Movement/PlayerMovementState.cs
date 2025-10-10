using UnityEngine;

namespace Adventure.Domain.Movement
{
    public sealed class PlayerMovementState
    {
        public Vector3 Velocity { get; private set; }
        public bool IsGrounded { get; private set; }
        public float Pitch { get; private set; }

        public PlayerMovementState(Vector3 initialVelocity, bool isGrounded, float initialPitch = 0f)
        {
            Velocity = initialVelocity;
            IsGrounded = isGrounded;
            Pitch = initialPitch;
        }

        public void SetGrounded(bool grounded) => IsGrounded = grounded;
        public void SetVelocity(Vector3 velocity) => Velocity = velocity;
        public void SetPitch(float pitch) => Pitch = pitch;
    }
}
