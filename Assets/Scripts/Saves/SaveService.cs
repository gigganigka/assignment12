using System;
using System.Linq;
using UnityEngine;
public static class SaveService
{
    static SaveData data;
    static SaveRepository repository;
    public static string Status { get; private set; }
    public static SaveData Data { get { EnsureLoaded(); return data; } }
    public static string FilePath { get { EnsureLoaded(); return repository.FilePath; } }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Clear() { data = null; repository = null; Status = null; }
    static void EnsureLoaded()
    {
        if (data != null) return;
        repository = new SaveRepository(Application.persistentDataPath);
        data = repository.Load(Defaults, Validate);
        Status = repository.LastError == null ? "" : "Save was unreadable. Defaults restored.";
    }
    public static SaveData Defaults()
    {
        GameCatalog c = GameCatalog.Current;
        return new SaveData {
            version=SaveRepository.SchemaVersion, game=SaveRepository.GameId,
            unlocked=new[]{c.levels[0].id}, completed=new int[0], currentLevel=c.levels[0].id,
            currentHp=c.player.maxHp, levelEntryHp=c.player.maxHp,
            musicVolume=c.defaults.musicVolume, soundVolume=c.defaults.soundVolume,
            hazardsEnabled=c.defaults.hazardsEnabled, enemiesEnabled=c.defaults.enemiesEnabled,
            healthSetting=0, fireCooldown=0, collectedCoins=new int[0],
            collectedHearts=new int[0], enemyHp=new int[0],enemyX=new float[0],enemyY=new float[0]
        };
    }
    public static bool Finite(float x) { return !float.IsNaN(x) && !float.IsInfinity(x); }
    public static bool Validate(SaveData s)
    {
        GameCatalog c=GameCatalog.Current;
        if (s.unlocked==null || s.completed==null || s.collectedCoins==null ||
            s.collectedHearts==null || s.enemyHp==null || s.enemyX==null || s.enemyY==null || c.Level(s.currentLevel)==null) return false;
        if (!s.unlocked.Contains(c.levels[0].id) || s.unlocked.Distinct().Count()!=s.unlocked.Length ||
            s.unlocked.Any(id=>c.Level(id)==null) || s.completed.Any(id=>c.Level(id)==null || !s.unlocked.Contains(id)) ||
            !s.unlocked.Contains(s.currentLevel)) return false;
        if (s.runScore<0 || s.bestScore<0 || s.levelEntryScore<0 || s.levelEntryScore>s.runScore) return false;
        if (!Finite(s.musicVolume) || !Finite(s.soundVolume) || s.musicVolume<0 || s.musicVolume>1 ||
            s.soundVolume<0 || s.soundVolume>1 || !Finite(s.fireCooldown)) return false;
        if (s.healthSetting!=0 && (s.healthSetting<c.defaults.minHp || s.healthSetting>c.defaults.maxHp)) return false;
        int max=s.healthSetting==0 ? c.player.maxHp : s.healthSetting;
        if(s.currentHp<0 || s.levelEntryHp<0) return false;
        s.currentHp=Math.Min(s.currentHp,max); s.levelEntryHp=Math.Min(s.levelEntryHp,max);
        if (s.fireCooldown!=0 && (s.fireCooldown<c.defaults.minFireCooldown || s.fireCooldown>c.defaults.maxFireCooldown)) return false;
        LevelData level=c.Level(s.currentLevel);
        if (!Finite(s.playerVelocityY) || !Finite(s.playerX) || !Finite(s.playerY) || !Finite(s.timeLeft) || s.timeLeft<0) return false;
        s.timeLeft=Math.Min(s.timeLeft,level.timeLimit);
        if (s.hasCheckpoint && (s.playerX<level.checkpointMin.x || s.playerX>level.checkpointMax.x ||
            s.playerY<level.checkpointMin.y || s.playerY>level.checkpointMax.y ||
            s.collectedCoins.Length!=level.coins.Length || s.collectedHearts.Length!=level.hearts.Length ||
            s.enemyHp.Length!=level.enemies.Length+(level.hasBoss?1:0) || s.enemyX.Length!=s.enemyHp.Length || s.enemyY.Length!=s.enemyHp.Length)) return false;
        if(Math.Abs(s.playerVelocityY)>1000) return false;
        if(s.hasCheckpoint && (s.enemyX.Any(x=>x<level.checkpointMin.x || x>level.checkpointMax.x) ||
            s.enemyY.Any(y=>y<level.checkpointMin.y || y>level.checkpointMax.y))) return false;
        if(s.enemyHp.Any(x=>x<0) || s.enemyX.Any(x=>!Finite(x)) || s.enemyY.Any(x=>!Finite(x))) return false;
        if (s.collectedCoins.Any(x=>x!=0 && x!=1) || s.collectedHearts.Any(x=>x!=0 && x!=1)) return false;
        return true;
    }
    public static bool Write()
    {
        EnsureLoaded();
        data.bestScore=Math.Max(data.bestScore,data.runScore);
        bool ok=repository.Write(data);
        Status=ok ? "Saved" : "Could not save. Check storage permissions.";
        if (ok) WebSaveSync.Flush();
        return ok;
    }
    public static SaveData CreateResetData(SaveData old)
    {
        SaveData reset=Defaults();
        reset.musicVolume=old.musicVolume; reset.soundVolume=old.soundVolume;
        reset.healthSetting=old.healthSetting; reset.fireCooldown=old.fireCooldown;
        reset.hazardsEnabled=old.hazardsEnabled; reset.enemiesEnabled=old.enemiesEnabled;
        reset.currentHp=reset.levelEntryHp=old.healthSetting==0?GameCatalog.Current.player.maxHp:old.healthSetting;
        return reset;
    }
    public static void ResetProgress()
    {
        EnsureLoaded(); data=CreateResetData(data); Write();
    }
}
