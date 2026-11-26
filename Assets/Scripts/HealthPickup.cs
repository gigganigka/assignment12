using UnityEngine;

// Сердце / аптечка: лечит героя. Collider2D (Is Trigger) + этот скрипт.
public class HealthPickup : MonoBehaviour
{
    public int healAmount;
    public AudioClip pickupSound;

    void Awake() { healAmount=GameCatalog.Current.items.healing; }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Health h = other.GetComponent<Health>();
        if (h != null) h.Heal(healAmount);
        Sfx.Play(SoundEvent.Heal);
        Destroy(gameObject);
    }
}
