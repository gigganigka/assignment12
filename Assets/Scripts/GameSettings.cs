using UnityEngine;
// Settings persist in JSON. A zero override means use the live data asset value.
public static class GameSettings
{
    public static int playerMaxHp
    {
        get { int value=SaveService.Data.healthSetting; return value==0?GameCatalog.Current.player.maxHp:value; }
        set {
            SaveData d=SaveService.Data;
            d.healthSetting=Mathf.Clamp(value,GameCatalog.Current.defaults.minHp,GameCatalog.Current.defaults.maxHp);
            d.currentHp=Mathf.Min(d.currentHp,d.healthSetting); d.levelEntryHp=Mathf.Min(d.levelEntryHp,d.healthSetting);
        }
    }
    public static float fireCooldown
    {
        get { float value=SaveService.Data.fireCooldown; return value==0?GameCatalog.Current.player.fireCooldown:value; }
        set { SaveService.Data.fireCooldown=Mathf.Clamp(value,GameCatalog.Current.defaults.minFireCooldown,GameCatalog.Current.defaults.maxFireCooldown); }
    }
    public static bool hazardsEnabled { get { return SaveService.Data.hazardsEnabled; } set { SaveService.Data.hazardsEnabled=value; } }
    public static bool enemiesEnabled { get { return SaveService.Data.enemiesEnabled; } set { SaveService.Data.enemiesEnabled=value; } }
}
