using UnityEngine;

public sealed class PatrollerBrain : EnemyBrain2D
{
    public enum State { Patrol, Chase, WindUp, Recover }
    [SerializeField] State state;
    [SerializeField, Min(0.1f)] float chaseSpeed;
    [SerializeField, Min(0)] float sightMemory;
    [SerializeField, Min(0.1f)] float chaseRange;
    float lastSeen;
    Vector2 lastKnownPosition;
    public override string StateName => state.ToString();
    protected override bool Telegraphing => state == State.WindUp;
    protected override void Awake()
    {
        base.Awake();
        if (tuning != null)
        {
            chaseSpeed = tuning.chaseSpeed;
            sightMemory = tuning.sightMemory;
            chaseRange = tuning.chaseRange;
        }
        chaseRange = Mathf.Max(visionRange, chaseRange);
    }
    void RememberTarget() { lastSeen = Time.time; lastKnownPosition = TargetPosition; }
    void Enter(State next)
    {
        state = next; enteredAt = Time.time; moveIntent = 0;
        if (next == State.WindUp) Sfx.Play(SoundEvent.PatrollerWindUp);
    }
    protected override void TickState()
    {
        switch (state)
        {
            case State.Patrol:
                Patrol();
                if (CanSeeTarget()) { RememberTarget(); Enter(State.Chase); }
                break;
            case State.Chase:
                bool visible = CanTrackTarget(chaseRange);
                if (visible) RememberTarget();
                if (Time.time - lastSeen > sightMemory) { Enter(State.Patrol); break; }
                FacePosition(lastKnownPosition);
                if (visible && DistanceToTarget <= attackRange && MayStartAttack) Enter(State.WindUp);
                else if (Mathf.Abs(lastKnownPosition.x - transform.position.x) > waypointTolerance)
                    MoveGrounded(facing * chaseSpeed);
                else moveIntent = 0;
                break;
            case State.WindUp:
                // Facing is locked during the warning: stepping behind evades the hit.
                if (!AttackVisible) { Enter(State.Recover); break; }
                if (StateAge >= windUp) { Strike(); Enter(State.Recover); }
                break;
            case State.Recover:
                if (StateAge >= cooldown)
                {
                    if (CanSeeTarget()) { RememberTarget(); Enter(State.Chase); }
                    else Enter(State.Patrol);
                }
                break;
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, tuning != null ? tuning.chaseRange : chaseRange);
    }
}
