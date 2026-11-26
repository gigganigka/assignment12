using UnityEngine;

[CreateAssetMenu(menuName = "Coinbound/Enemy tuning")]
public sealed class EnemyTuning : ScriptableObject
{
    [Min(1)] public int hitPoints;
    [Min(0)] public float moveSpeed;
    [Min(0.1f)] public float visionRange;
    [Range(1, 360)] public float visionAngle;
    [Range(0.3f, 0.8f)] public float windUp;
    [Min(0.1f)] public float cooldown;
    [Min(1)] public int damage;
    [Min(0.1f)] public float attackRange;
    [Min(0.1f)] public float chaseSpeed;
    [Min(0)] public float sightMemory;
    [Min(0.1f)] public float chaseRange;
    [Min(0.1f)] public float bulletSpeed;
    [Min(0.1f)] public float bulletLifetime;
    [Min(0.1f)] public float chargeSpeed;
    [Min(0.1f)] public float chargeDuration;
    public Vector2 eyeOffset, muzzleOffset, colliderSize;
    public float waypointTolerance, ledgeProbeForward, ledgeProbeDepth, groundProbeLift;
    public float awarenessRange, facingDeadZone, wallProbe, hitFlashTime, screenMargin;
    public float artHeight, gravityScale;
    void OnValidate()
    {
        hitPoints = Mathf.Max(1, hitPoints); damage = Mathf.Max(1, damage);
        windUp = Mathf.Clamp(windUp, 0.3f, 0.8f);
        cooldown = Mathf.Max(0.1f, cooldown);
        chaseRange = Mathf.Max(visionRange, chaseRange);
    }
}
