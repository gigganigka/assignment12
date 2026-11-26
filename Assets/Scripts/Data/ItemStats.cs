using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/Item values")]
public sealed class ItemStats : ScriptableObject
{
    public int coinScore, healing, hazardDamage, levelClearScore, scorePerSecond;
    public float pickupRadius, pickupHeight;
}
