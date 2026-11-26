using UnityEngine;
public sealed class DataPickup : MonoBehaviour
{
    public DataLevelController owner;
    public int index;
    public bool heart;
    bool taken;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag("Player")) return;
        Health hp=other.GetComponent<Health>();
        if (hp==null || hp.CurrentHp<=0 || (heart && hp.CurrentHp>=hp.maxHp)) return;
        taken=true; owner.Collect(index,heart,hp);
        Sfx.Play(heart ? SoundEvent.Heal : SoundEvent.Pickup);
        Destroy(gameObject);
    }
}
