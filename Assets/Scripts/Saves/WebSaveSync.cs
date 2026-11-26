using System.Runtime.InteropServices;
public static class WebSaveSync
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void CoinboundFlushSave();
#endif
    public static void Flush()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CoinboundFlushSave();
#endif
    }
}
