using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/Boss stats")]
public sealed class BossStats : ScriptableObject
{
    public int maxHp, contactDamage, fanCount, phaseTwoFanCount, rainCount, rainColumns, phaseTwoColumns;
    public float initialDelay, restTime, windUp, spread, phaseTwoRestFactor, phaseTwoSpreadFactor;
    public float phaseTwoHpFraction, rainWarning, rainWidth, rainSpeed, rainOffsetFactor;
    public float rainProjectileSpacing, rainTopPadding, deathDelay, bulletSpeed, bulletLifetime;
    public int bulletDamage;
}
