using UnityEngine;
using UnityEngine.SceneManagement;
// Also fixes direct transitions between the retained original scene files.
public static class LegacyHealthCarry
{
    static int? pendingHp;
    static string destination;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { pendingHp=null; destination=null; }
    static bool LegacyLevel(string s)
    {
        return s==SceneNames.Level1 || s==SceneNames.Level2 || s==SceneNames.Boss || s==SceneNames.EnemyLab;
    }
    public static void BeforeLoad(string next)
    {
        string current=SceneManager.GetActiveScene().name;
        Health hero=Object.FindFirstObjectByType<Health>();
        if (LegacyLevel(current) && LegacyLevel(next) && current!=next && hero!=null && hero.CurrentHp>0)
        { pendingHp=hero.CurrentHp; destination=next; }
        else Reset();
    }
    public static int InitialHp(string scene,int maximum)
    {
        int hp=pendingHp.HasValue && destination==scene ? Mathf.Clamp(pendingHp.Value,1,maximum) : maximum;
        Reset(); return hp;
    }
}
