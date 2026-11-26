using System;
using UnityEngine;
[CreateAssetMenu(menuName="Coinbound/Data/Level")]
public sealed class LevelData : ScriptableObject
{
    [Serializable] public struct Surface { public Vector2 center, size; public bool oneWay; }
    [Serializable] public struct EnemySpawn
    {
        public EnemySpawner2D.Kind kind;
        public Vector2 position;
        public float patrolRadius;
        public EnemyTuning stats;
    }
    public int id;
    public string title;
    public Vector2 playerStart, exit, checkpointMin, checkpointMax;
    public float timeLimit, fallY;
    public int coinsRequired;
    public bool defeatAllEnemies;
    public Surface[] surfaces;
    public EnemySpawn[] enemies;
    public Vector2[] coins, hearts, hazards;
    public Vector2 hazardSize;
    public bool hasBoss;
    public Vector2 bossPosition;
    public Vector2 bossSize;
}
