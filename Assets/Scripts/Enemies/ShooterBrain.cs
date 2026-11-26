using UnityEngine;

public sealed class ShooterBrain : EnemyBrain2D
{
    public enum State { Idle, Aim, Shoot, Cooldown }
    [SerializeField] State state;
    [SerializeField, Min(0.1f)] float bulletSpeed;
    [SerializeField, Min(0.1f)] float bulletLifetime;
    [SerializeField] Vector2 muzzleOffset;
    [SerializeField] Sprite projectileSprite;
    Vector2 aim;
    public override string StateName => state.ToString();
    protected override bool Telegraphing => state == State.Aim;
    protected override void Awake()
    {
        base.Awake();
        if (tuning == null) return;
        bulletSpeed = tuning.bulletSpeed;
        bulletLifetime = tuning.bulletLifetime;
        muzzleOffset=tuning.muzzleOffset;
    }
    void Enter(State next)
    {
        state = next; enteredAt = Time.time; moveIntent = 0;
        if (next == State.Aim) Sfx.Play(SoundEvent.ShooterWindUp);
    }
    protected override void TickState()
    {
        switch (state)
        {
            case State.Idle:
                FaceTarget();
                if (MayStartAttack)
                {
                    aim = (TargetPosition - (Vector2)transform.position).normalized;
                    Enter(State.Aim);
                }
                break;
            case State.Aim:
                if (!AttackVisible) { Enter(State.Cooldown); break; }
                if (StateAge >= windUp) Enter(State.Shoot);
                break;
            case State.Shoot:
                if (MayStartAttack)
                {
                    Vector2 origin = (Vector2)transform.position + new Vector2(muzzleOffset.x * facing, muzzleOffset.y);
                    EnemyShot2D.Spawn(this, origin, aim, bulletSpeed, bulletLifetime, damage, projectileSprite);
                    Sfx.Play(SoundEvent.EnemyShot);
                }
                Enter(State.Cooldown);
                break;
            case State.Cooldown:
                if (StateAge >= cooldown) Enter(State.Idle);
                break;
        }
    }
}
