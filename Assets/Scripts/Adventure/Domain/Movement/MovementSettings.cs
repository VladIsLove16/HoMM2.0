namespace Adventure.Domain.Movement
{
    public sealed class MovementSettings
    {
        public float MoveSpeed { get; }
        public float SprintMultiplier { get; }
        public float Acceleration { get; }
        public float Gravity { get; }
        public float StepHeight { get; }
        public float GroundCheckDistance { get; }
        public float LookSensitivity { get; }
        public float MaxLookPitch { get; }

        public MovementSettings(
            float moveSpeed,
            float sprintMultiplier,
            float acceleration,
            float gravity,
            float stepHeight,
            float groundCheckDistance,
            float lookSensitivity,
            float maxLookPitch)
        {
            MoveSpeed = moveSpeed;
            SprintMultiplier = sprintMultiplier;
            Acceleration = acceleration;
            Gravity = gravity;
            StepHeight = stepHeight;
            GroundCheckDistance = groundCheckDistance;
            LookSensitivity = lookSensitivity;
            MaxLookPitch = maxLookPitch;
        }
    }
}
