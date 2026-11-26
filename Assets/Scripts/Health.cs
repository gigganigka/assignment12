using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Здоровье героя. Висит на Player.
public class Health : MonoBehaviour
{
    public int maxHp;                 // по заданию HP героя = 3
    public bool useMenuSettings = true;   // true: берём HP из настроек меню
    public float invincibleTime;   // сколько секунд неуязвим после удара
    public float blinkInterval;    // как часто мигает
    public AudioClip damageSound;

    public int CurrentHp { get; private set; }
    public bool IsInvincible { get; private set; }

    SpriteRenderer sr;
    bool dead;                            // чтобы смерть не сработала два раза

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        PlayerStats stats=GameCatalog.Current.player;
        maxHp=GameSettings.playerMaxHp;
        invincibleTime=stats.invincibility; blinkInterval=stats.blinkInterval;
        CurrentHp=gameObject.scene.name==SceneNames.Campaign && SaveService.Data.runActive
            ? Mathf.Clamp(SaveService.Data.currentHp,0,maxHp)
            : LegacyHealthCarry.InitialHp(gameObject.scene.name,maxHp);
    }

    public void TakeDamage(int amount)
    {
        if (dead || IsInvincible || amount<=0) return;   // пока неуязвим - урон не проходит
        CurrentHp = Mathf.Max(0,CurrentHp-amount);
        RememberHealth();
        Sfx.Play(SoundEvent.Hit);
        if (CurrentHp <= 0) { Die(); return; }
        StartCoroutine(InvincibilityRoutine());
    }

    public void Heal(int amount)
    {
        if (dead || amount<=0) return;
        CurrentHp = Mathf.Min(maxHp, CurrentHp + amount); // не больше максимума
        RememberHealth();
    }

    // Смерть (HP = 0 или падение в яму)
    public void Die()
    {
        if (dead) return;
        dead = true;
        SoundPass.Ensure(); SoundPass.Instance.PlayerDied();
        CurrentHp = 0;
        RememberHealth();
        if(gameObject.scene.name==SceneNames.Campaign) SaveService.Write();

        StopAllCoroutines();              // останавливаем мигание
        sr.enabled = true;

        // Отключаем управление, чтобы мёртвый герой не бегал и не стрелял
        PlayerInput input = GetComponent<PlayerInput>();
        if (input != null) input.enabled = false;

        if (MobileGameUI.Instance != null)
            MobileGameUI.Instance.ShowDeath();
        else if (DeathScreen.Instance != null)
            DeathScreen.Instance.Show();  // показываем экран смерти (Restart / Main Menu)
        else
            RestartFallback(); // запасной вариант: перезапуск уровня
    }

    void RememberHealth()
    {
        if(gameObject.scene.name==SceneNames.Campaign && SaveService.Data.runActive)
            SaveService.Data.currentHp=CurrentHp;
    }
    void RestartFallback()
    {
        if(gameObject.scene.name==SceneNames.Campaign) RunFlow.Restart();
        else GameScenes.Load(SceneManager.GetActiveScene().name);
    }
    // Корутина: код, который "растянут" во времени (yield return = подождать)
    IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        float t = 0f;
        while (t < invincibleTime)
        {
            sr.enabled = !sr.enabled;                    // мигаем: вкл/выкл картинку
            yield return new WaitForSeconds(blinkInterval);
            t += blinkInterval;
        }
        sr.enabled = true;
        IsInvincible = false;
    }
}
