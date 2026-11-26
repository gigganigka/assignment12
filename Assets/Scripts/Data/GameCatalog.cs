using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/Game catalog")]
public sealed class GameCatalog : ScriptableObject
{
    public PlayerStats player;
    public ItemStats items;
    public AppDefaults defaults;
    public BossStats boss;
    public EnemyTuning[] enemyStats;
    public LevelData[] levels;
    public GameObject fireballPrefab;
    public Sprite coinSprite, bossSprite;
    static GameCatalog cached;
    public static GameCatalog Current
    {
        get
        {
            if (cached == null) cached = Resources.Load<GameCatalog>("Data/GameCatalog");
            if (cached == null) throw new System.InvalidOperationException("Missing Resources/Data/GameCatalog.asset");
            return cached;
        }
    }
    public LevelData Level(int id)
    {
        foreach (LevelData level in levels) if (level != null && level.id == id) return level;
        return null;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Clear() { cached = null; }
}
