using System;
using System.Linq;
// Deterministic state transitions; no scene loads and no file I/O.
public static class RunProgress
{
    public enum Completion { Invalid, NextLevel, Finished }
    public static void ClearCheckpoint(SaveData d)
    {
        d.hasCheckpoint=false; d.collectedCoins=new int[0]; d.collectedHearts=new int[0]; d.enemyHp=new int[0];
        d.playerX=d.playerY=d.playerVelocityY=d.timeLeft=0; d.enemyX=new float[0]; d.enemyY=new float[0];
    }
    public static void Prepare(SaveData d,int id,int hp,int score,int maximum)
    {
        d.currentLevel=id; d.runActive=true;
        d.currentHp=d.levelEntryHp=Math.Max(1,Math.Min(maximum,hp));
        d.runScore=d.levelEntryScore=score; ClearCheckpoint(d);
    }
    public static Completion Complete(SaveData d,GameCatalog catalog,LevelData level,int hp,float seconds,int maximum)
    {
        if(hp<=0 || !d.runActive || d.currentLevel!=level.id) return Completion.Invalid;
        int index=Array.IndexOf(catalog.levels,level);
        if(index<0) return Completion.Invalid;
        d.runScore+=catalog.items.levelClearScore+(int)Math.Floor(Math.Max(0,seconds))*catalog.items.scorePerSecond;
        d.bestScore=Math.Max(d.bestScore,d.runScore);
        if(!d.completed.Contains(level.id)) d.completed=d.completed.Concat(new[]{level.id}).ToArray();
        if(index+1<catalog.levels.Length)
        {
            int next=catalog.levels[index+1].id;
            if(!d.unlocked.Contains(next)) d.unlocked=d.unlocked.Concat(new[]{next}).ToArray();
            Prepare(d,next,hp,d.runScore,maximum);
            return Completion.NextLevel;
        }
        d.currentHp=Math.Min(hp,maximum); d.runActive=false; ClearCheckpoint(d);
        return Completion.Finished;
    }
}
