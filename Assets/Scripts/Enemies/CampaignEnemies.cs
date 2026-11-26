using System;
using UnityEngine;

// Authored in level1 / level2; keeps the original terrain, coins, hazards and exit.
[DefaultExecutionOrder(-900)]
public sealed class CampaignEnemies : MonoBehaviour
{
    [Serializable]
    public struct SpawnPoint
    {
        public EnemySpawner2D.Kind kind;
        public Vector2 position;
        [Min(0)] public float patrolRadius;
    }
    [SerializeField] SpawnPoint[] spawns;
    public EnemyBrain2D[] Enemies { get; private set; }
    void Awake()
    {
        // Replace legacy contact-only patrols so there are no duplicate enemies.
        foreach (PatrolEnemy legacy in FindObjectsByType<PatrolEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (legacy.gameObject.scene != gameObject.scene) continue;
            legacy.gameObject.SetActive(false);
            Destroy(legacy.gameObject);
        }
        if (!GameSettings.enemiesEnabled || spawns == null)
        {
            Enemies = new EnemyBrain2D[0];
            return;
        }
        Enemies = new EnemyBrain2D[spawns.Length];
        for (int i = 0; i < spawns.Length; i++)
            Enemies[i] = EnemySpawner2D.Spawn(spawns[i].kind, spawns[i].position, spawns[i].patrolRadius, transform);
    }
}
