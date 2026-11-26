using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/App defaults")]
public sealed class AppDefaults : ScriptableObject
{
    public float musicVolume, soundVolume;
    public float jumpVolume, hurtVolume, coinVolume, enemyHitVolume, enemyShotVolume;
    public bool hazardsEnabled, enemiesEnabled;
    public int minHp, maxHp;
    public float minFireCooldown, maxFireCooldown, cooldownStep, volumeStep;
}
