using Adventure.Domain.Movement;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Movement Settings", fileName = "MovementSettings")]
public sealed class MovementSettingsSO : ScriptableObject
{
    [Header("Speed")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float acceleration = 10f;

    [Header("Vertical")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float stepHeight = 0.5f;
    [SerializeField] private float groundSnapDistance = 0.2f;

    [Header("Camera")]
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float maxLookPitch = 80f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;

    public float LookSensitivity => lookSensitivity;
    public float StepHeight => stepHeight;
    public LayerMask CollisionMask => collisionMask;

    public MovementSettings ToDomain()
    {
        return new MovementSettings(
            moveSpeed,
            sprintMultiplier,
            acceleration,
            gravity,
            stepHeight,
            groundSnapDistance,
            lookSensitivity,
            maxLookPitch);
    }
}
