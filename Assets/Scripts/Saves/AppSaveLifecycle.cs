using UnityEngine;
public sealed class AppSaveLifecycle : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SaveData loaded=SaveService.Data;
        GameObject go=new GameObject("AppSaveLifecycle");
        DontDestroyOnLoad(go); go.AddComponent<AppSaveLifecycle>();
    }
    void Save()
    {
        if (DataLevelController.Instance!=null) DataLevelController.Instance.CaptureCheckpoint();
        SaveService.Write();
    }
    void OnApplicationPause(bool paused) { if(paused) Save(); }
    void OnApplicationFocus(bool focused) { if(!focused) Save(); }
    void OnApplicationQuit() { Save(); }
}
