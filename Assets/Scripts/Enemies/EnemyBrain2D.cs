using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public abstract class EnemyBrain2D : MonoBehaviour, IDamageable
{
    [Header("Health and movement")]
    [SerializeField, Min(1)] protected int hitPoints;
    [SerializeField, Min(0)] protected float moveSpeed;
    [SerializeField] protected Transform[] waypoints;
    [SerializeField, Min(0.01f)] protected float waypointTolerance;
    [SerializeField, Min(0.01f)] protected float ledgeProbeForward;
    [SerializeField, Min(0.01f)] protected float ledgeProbeDepth;
    [Header("Vision and attack")]
    [SerializeField, Min(0.1f)] protected float visionRange;
    [SerializeField, Range(1, 360)] protected float visionAngle;
    [SerializeField] protected Vector2 eyeOffset;
    [SerializeField] protected LayerMask walls = 1 << 6;
    [SerializeField, Range(0.3f, 0.8f)] protected float windUp;
    [SerializeField, Min(0.1f)] protected float cooldown;
    [SerializeField, Min(1)] protected int damage;
    [SerializeField, Min(0)] protected float attackRange;
    [SerializeField, Range(0, 0.2f)] float screenMargin;
    [SerializeField, Min(0)] float hitFlashTime;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warningColor = new Color(1, 0.22f, 0.1f);
    [SerializeField] string currentState;

    protected Rigidbody2D body;
    protected BoxCollider2D shape;
    protected SpriteRenderer visual;
    protected Health target;
    Collider2D targetShape;
    public EnemyTuning tuning;
    protected Camera gameCamera;
    protected float enteredAt;
    protected float moveIntent;
    protected int facing = -1;
    int waypointIndex;
    float flashUntil;
    TMP_Text stateLabel;
    public abstract string StateName { get; }
    protected abstract bool Telegraphing { get; }
    protected abstract void TickState();
    protected float StateAge => Time.time - enteredAt;
    protected Vector2 TargetPosition => targetShape.bounds.center;
    protected float DistanceToTarget => target == null ? float.PositiveInfinity : Vector2.Distance(shape.bounds.center, TargetPosition);
    public bool HasLivingTarget => target != null && targetShape != null && target.CurrentHp > 0;

    protected virtual void Awake()
    {
        if(tuning==null) tuning=GameCatalog.Current.enemyStats[0];
        if (tuning != null)
        {
            hitPoints = tuning.hitPoints; moveSpeed = tuning.moveSpeed;
            visionRange = tuning.visionRange; visionAngle = tuning.visionAngle;
            windUp = Mathf.Clamp(tuning.windUp, 0.3f, 0.8f); cooldown = Mathf.Max(0.1f, tuning.cooldown);
            damage = tuning.damage; attackRange = tuning.attackRange;
            waypointTolerance=tuning.waypointTolerance; ledgeProbeForward=tuning.ledgeProbeForward;
            ledgeProbeDepth=tuning.ledgeProbeDepth; eyeOffset=tuning.eyeOffset;
            hitFlashTime=tuning.hitFlashTime; screenMargin=tuning.screenMargin;
        }
        body = GetComponent<Rigidbody2D>();
        shape = GetComponent<BoxCollider2D>();
        visual = GetComponent<SpriteRenderer>();
        gameCamera = Camera.main;
        PlayerController hero = FindFirstObjectByType<PlayerController>();
        if (hero != null)
        {
            target = hero.GetComponent<Health>();
            targetShape = hero.GetComponent<Collider2D>();
            foreach (Collider2D c in hero.GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(shape, c);
        }
        stateLabel = GetComponentInChildren<TMP_Text>();
        enteredAt = Time.time;
        if (!GameSettings.enemiesEnabled) gameObject.SetActive(false);
    }
    void Update()
    {
        if (Time.timeScale <= 0 || !HasLivingTarget) { moveIntent = 0; return; }
        TickState();
        currentState = StateName;
        visual.color = Telegraphing ? warningColor : (Time.time < flashUntil ? Color.white : normalColor);
        visual.flipX = facing < 0;
        if (stateLabel != null)
        {
            stateLabel.text = name + "\n" + StateName + (Telegraphing ? " !" : "");
            stateLabel.color = Telegraphing ? warningColor : Color.white;
        }
    }
    void FixedUpdate()
    {
        body.linearVelocity = new Vector2(moveIntent, body.linearVelocity.y);
    }
    protected void FaceTarget()
    {
        if (target != null) FacePosition(TargetPosition);
    }
    protected void FacePosition(Vector2 position)
    {
        float dx = position.x - shape.bounds.center.x;
        if (Mathf.Abs(dx) > tuning.facingDeadZone) facing = dx < 0 ? -1 : 1;
    }
    protected void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) { moveIntent = 0; return; }
        Transform point = waypoints[waypointIndex];
        if (point == null) { moveIntent = 0; return; }
        float dx = point.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= waypointTolerance)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            moveIntent = 0;
            return;
        }
        MoveGrounded(Mathf.Sign(dx) * moveSpeed);
    }
    protected bool MoveGrounded(float speed)
    {
        if (Mathf.Abs(speed) < 0.001f) { moveIntent = 0; return true; }
        facing = speed < 0 ? -1 : 1;
        Vector2 foot = new Vector2(shape.bounds.center.x + facing * ledgeProbeForward, shape.bounds.min.y + tuning.groundProbeLift);
        bool ground = Physics2D.Raycast(foot, Vector2.down, ledgeProbeDepth, walls);
        bool blocked = Physics2D.Raycast(shape.bounds.center, Vector2.right * facing,
            shape.bounds.extents.x + tuning.wallProbe, walls);
        moveIntent = ground && !blocked ? speed : 0;
        return ground && !blocked;
    }
    public bool IsOnScreen(Vector3 point)
    {
        if (gameCamera == null) return false;
        Vector3 v = gameCamera.WorldToViewportPoint(point);
        return v.z > 0 && v.x >= screenMargin && v.x <= 1 - screenMargin &&
               v.y >= screenMargin && v.y <= 1 - screenMargin;
    }
    public bool AttackVisible => HasLivingTarget && IsOnScreen(transform.position) && IsOnScreen(TargetPosition);
    protected bool CanSeeTarget()
    {
        if (!CanTrackTarget(visionRange)) return false;
        Vector2 delta = TargetPosition - EyePosition;
        // Nearby footsteps also reveal a player who crossed behind the enemy.
        return delta.sqrMagnitude <= tuning.awarenessRange * tuning.awarenessRange ||
               Vector2.Angle(Vector2.right * facing, delta) <= visionAngle * 0.5f;
    }
    protected Vector2 EyePosition => (Vector2)shape.bounds.center + eyeOffset;
    protected bool CanTrackTarget(float range)
    {
        if (!HasLivingTarget) return false;
        Vector2 delta = TargetPosition - EyePosition;
        if (delta.sqrMagnitude > range * range) return false;
        // Retaining an acquired target ignores facing, but never ignores walls.
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(walls); filter.useTriggers = false;
        return Physics2D.Raycast(EyePosition, delta.normalized, filter, visionHits, delta.magnitude) == 0;
    }
    readonly RaycastHit2D[] visionHits = new RaycastHit2D[1];
    protected bool MayStartAttack => AttackVisible && CanSeeTarget();
    protected void Strike()
    {
        Sfx.Play(SoundEvent.EnemyStrike);
        if (MayStartAttack && Vector2.Dot(Vector2.right * facing, TargetPosition - (Vector2)shape.bounds.center) >= 0 &&
            DistanceToTarget <= attackRange) target.TakeDamage(damage);
    }
    public int CurrentHp => hitPoints;
    public void RestoreHealth(int hp) { hitPoints=Mathf.Clamp(hp,1,tuning.hitPoints); }
    public void TakeHit(int amount)
    {
        if (amount <= 0 || hitPoints <= 0) return;
        hitPoints -= amount;
        flashUntil = Time.time + hitFlashTime;
        Sfx.Play(hitPoints <= 0 ? SoundEvent.EnemyDeath : SoundEvent.EnemyHit);
        if (hitPoints <= 0) Destroy(gameObject);
    }
    public void SetStateLabel(TMP_Text label) { stateLabel = label; }
    public void SetWaypoints(Transform[] points) { waypoints = points; }
    protected virtual void OnValidate()
    {
        windUp = Mathf.Clamp(windUp, 0.3f, 0.8f);
        cooldown = Mathf.Max(0.1f, cooldown);
        hitPoints = Mathf.Max(1, hitPoints);
    }
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, tuning != null ? tuning.visionRange : visionRange);
        if (target != null) Gizmos.DrawLine((Vector2)transform.position + eyeOffset, TargetPosition);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
