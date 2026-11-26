using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class Assignment11Tools
{
    static int passed;
    static void Check(bool condition,string name)
    {
        if(!condition) throw new InvalidOperationException("Assignment 11 check failed: "+name);
        passed++; Debug.Log("PASS "+name);
    }
    [MenuItem("Tools/Assignment 11/Open Main Menu")]
    static void OpenMenu()
    {
        if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/mainmenu.unity");
    }
    [MenuItem("Tools/Assignment 11/Open Save Folder")]
    static void OpenSaveFolder()
    {
        Directory.CreateDirectory(Application.persistentDataPath);
        EditorUtility.RevealInFinder(Application.persistentDataPath);
        Debug.Log("save.json directory: "+Application.persistentDataPath);
    }
    [MenuItem("Tools/Assignment 11/Check Keyboard Control")]
    static void InputStatus()
    {
        PlayerController hero=UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        Health hp=hero!=null?hero.GetComponent<Health>():null;
        Debug.Log("Input check: keyboard="+(UnityEngine.InputSystem.Keyboard.current!=null)+
            ", player="+(hero!=null)+", active="+(hero!=null && hero.isActiveAndEnabled)+
            ", moveSpeed="+(hero!=null?hero.moveSpeed:0)+", HP="+(hp!=null?hp.CurrentHp:0)+
            ", timeScale="+Time.timeScale+". Click the Game view before typing in the editor.");
    }
    [MenuItem("Tools/Assignment 11/Run Data and Save Checks")]
    public static void Verify()
    {
        passed=0;
        GameCatalog c=GameCatalog.Current;
        Check(c.player!=null && c.items!=null && c.defaults!=null && c.boss!=null,"Catalog balance references");
        Check(c.player.maxHp>0 && c.player.jumpVelocity>0 && c.player.moveSpeed>0 && c.player.blinkInterval>0,"Player stats");
        Check(c.levels.Length==3 && c.levels.Select(x=>x.id).Distinct().Count()==3,"Three distinct LevelData assets");
        Check(c.fireballPrefab!=null && c.coinSprite!=null && c.bossSprite!=null,"Art and projectile references");
        foreach(LevelData level in c.levels)
        {
            Check(level.timeLimit>0 && level.coinsRequired<=level.coins.Length && level.surfaces.Length>0,"Level "+level.id+" data");
            Check(level.enemies.All(x=>x.stats!=null && x.stats.hitPoints>0 && x.stats.windUp>=0.3f && x.stats.windUp<=0.8f),"Level "+level.id+" enemy stats");
        }
        string temp=Path.Combine(Path.GetTempPath(),"CoinboundSaveTests_"+Guid.NewGuid().ToString("N"));
        SaveRepository repository=new SaveRepository(temp);
        try
        {
            SaveData missing=repository.Load(SaveService.Defaults,SaveService.Validate);
            Check(!missing.runActive && missing.unlocked.Length==1,"Missing file -> defaults");
            SaveData run=SaveService.Defaults();
            RunProgress.Prepare(run,c.levels[0].id,1,0,c.player.maxHp);
            RunProgress.Completion next=RunProgress.Complete(run,c,c.levels[0],1,10,c.player.maxHp);
            Check(next==RunProgress.Completion.NextLevel && run.currentHp==1 && run.levelEntryHp==1,"Transition preserves 1 HP");
            Check(run.unlocked.Contains(c.levels[1].id) && run.completed.Contains(c.levels[0].id),"Completion unlocks next level");
            Check(repository.Write(run),"JSON write");
            SaveData loaded=repository.Load(SaveService.Defaults,SaveService.Validate);
            Check(loaded.currentHp==1 && loaded.currentLevel==c.levels[1].id && loaded.bestScore==run.bestScore,"Reload preserves HP/progress/score");
            int replacementHp=Math.Min(2,c.player.maxHp); run.currentHp=replacementHp; Check(repository.Write(run),"Replace existing JSON");
            Check(repository.Load(SaveService.Defaults,SaveService.Validate).currentHp==replacementHp,"Replacement file reload");
            File.WriteAllText(repository.FilePath,"{ deliberately broken");
            SaveData broken=repository.Load(SaveService.Defaults,SaveService.Validate);
            Check(!broken.runActive && broken.unlocked.Length==1 && repository.LastError!=null,"Broken file -> defaults");
            run.version=SaveRepository.SchemaVersion+1; repository.Write(run);
            Check(!repository.Load(SaveService.Defaults,SaveService.Validate).runActive,"Unsupported schema -> defaults");
            run.version=SaveRepository.SchemaVersion; run.currentHp=-1; repository.Write(run);
            Check(!repository.Load(SaveService.Defaults,SaveService.Validate).runActive,"Invalid HP -> defaults");
            File.WriteAllText(repository.FilePath,"{}");
            Check(!repository.Load(SaveService.Defaults,SaveService.Validate).runActive,"Missing fields -> defaults");
            run=SaveService.Defaults(); RunProgress.Prepare(run,c.levels[0].id,1,0,c.player.maxHp);
            Check(RunProgress.Complete(run,c,c.levels[0],0,10,c.player.maxHp)==RunProgress.Completion.Invalid && run.currentLevel==c.levels[0].id,"Dead player cannot complete level");
            run.musicVolume=0.4f;run.bestScore=1200;run.soundVolume=0.2f;
            SaveData reset=SaveService.CreateResetData(run);
            Check(reset.bestScore==0 && !reset.runActive && reset.unlocked.Length==1 && reset.musicVolume==run.musicVolume && reset.soundVolume==run.soundVolume,"Reset clears progress, retains settings");
            int score=run.levelEntryScore;
            RunProgress.Prepare(run,run.currentLevel,c.player.maxHp,score,c.player.maxHp);
            Check(run.currentHp==c.player.maxHp && !run.hasCheckpoint && run.runScore==score,"Explicit restart resets attempt, no duplicate score");
        }
        finally { if(Directory.Exists(temp)) Directory.Delete(temp,true); }
        Debug.Log("Assignment 11: "+passed+" data/save checks passed. Real Unity gameplay and phone restart tests are still required.");
    }
}
