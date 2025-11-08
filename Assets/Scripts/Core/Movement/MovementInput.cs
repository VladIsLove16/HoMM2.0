namespace Adventure.Domain.Movement
{
    public readonly struct MovementInput
    {
        public MovementInput(UnityEngine.Vector2 move, UnityEngine.Vector2 look, bool sprint)
        {
            Move = move;
            Look = look;
            Sprint = sprint;
        }

        public UnityEngine.Vector2 Move { get; }
        public UnityEngine.Vector2 Look { get; }
        public bool Sprint { get; }

        public static MovementInput Empty => new MovementInput(UnityEngine.Vector2.zero, UnityEngine.Vector2.zero, false);
    }
}
