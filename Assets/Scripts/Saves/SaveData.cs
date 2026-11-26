using System;
[Serializable]
public sealed class SaveData
{
    public int version;
    public string game;
    public int bestScore, runScore, levelEntryScore;
    public int[] unlocked, completed;
    public int currentLevel, currentHp, levelEntryHp;
    public bool runActive, hasCheckpoint;
    public float playerX, playerY, playerVelocityY, timeLeft;
    public int[] collectedCoins, collectedHearts, enemyHp;
    public float[] enemyX, enemyY;
    public float musicVolume, soundVolume, fireCooldown;
    public int healthSetting;
    public bool hazardsEnabled, enemiesEnabled;
}
