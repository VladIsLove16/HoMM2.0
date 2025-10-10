using UnityEngine;

namespace Adventure.Domain.Movement
{
    public readonly struct MovementCommand
    {
        public MovementCommand(Vector3 desiredVelocity, float desiredPitch, bool interactRequested, bool collectRequested)
        {
            DesiredVelocity = desiredVelocity;
            DesiredPitch = desiredPitch;
            InteractRequested = interactRequested;
            CollectRequested = collectRequested;
        }

        public Vector3 DesiredVelocity { get; }
        public float DesiredPitch { get; }
        public bool InteractRequested { get; }
        public bool CollectRequested { get; }
    }
}
