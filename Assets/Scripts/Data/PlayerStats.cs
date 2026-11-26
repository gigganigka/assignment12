using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/Player stats")]
public sealed class PlayerStats : ScriptableObject
{
    public int maxHp;
    public float moveSpeed, jumpVelocity, gravityScale, invincibility, blinkInterval;
    public float groundRadius, groundedVelocityLimit, fireCooldown, bulletSpeed, bulletLifetime;
    public int bulletDamage;
}
