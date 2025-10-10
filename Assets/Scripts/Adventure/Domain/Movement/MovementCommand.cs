using UnityEngine;

namespace Adventure.Domain.Movement
{
    public readonly struct MovementCommand
    {
        public MovementCommand(Vector3 velocity, float desiredPitch)
        {
            Velocity = velocity;
            DesiredPitch = desiredPitch;
        }

        public Vector3 Velocity { get; }
        public float DesiredPitch { get; }
    }
}
