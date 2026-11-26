using System.Linq;
using UnityEngine;
public static class RunFlow
{
    public static void ClearCheckpoint() { RunProgress.ClearCheckpoint(SaveService.Data); }
    public static void PrepareAttempt(int id,int hp,int score)
    { RunProgress.Prepare(SaveService.Data,id,hp,score,GameSettings.playerMaxHp); }
    static void LoadCampaign() { SaveService.Write(); GameScenes.Load(SceneNames.Campaign,false); }
    public static void NewGame()
    {
        PrepareAttempt(GameCatalog.Current.levels[0].id,GameSettings.playerMaxHp,0);
        LoadCampaign();
    }
    public static void Continue()
    {
        SaveData d=SaveService.Data;
        if (!d.runActive) { NewGame(); return; }
        if (d.currentHp<=0) { Restart(); return; }
        LoadCampaign();
    }
    public static void SelectLevel(int id)
    {
        if (!SaveService.Data.unlocked.Contains(id)) return;
        PrepareAttempt(id,GameSettings.playerMaxHp,0); LoadCampaign();
    }
    public static void Restart()
    {
        SaveData d=SaveService.Data;
        PrepareAttempt(d.currentLevel,GameSettings.playerMaxHp,d.levelEntryScore); LoadCampaign();
    }
    public static void Complete(LevelData level,int remainingHp,float timeLeft)
    {
        RunProgress.Completion result=RunProgress.Complete(SaveService.Data,GameCatalog.Current,level,remainingHp,timeLeft,GameSettings.playerMaxHp);
        if(result==RunProgress.Completion.Invalid) return;
        if (result == RunProgress.Completion.NextLevel) Sfx.Play(SoundEvent.LevelClear);
        SaveService.Write();
        GameScenes.Load(result==RunProgress.Completion.NextLevel?SceneNames.Campaign:SceneNames.Win,false);
    }
    public static void SaveCurrent()
    {
        if (DataLevelController.Instance!=null) DataLevelController.Instance.CaptureCheckpoint();
        SaveService.Write();
    }
}
