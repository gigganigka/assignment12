using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
// All audio lives here: unloading a scene cannot cut a cue or restart a loop.
public sealed class SoundPass : MonoBehaviour
{
    public static SoundPass Instance { get; private set; }
    public SoundBank Bank { get; private set; }
    AudioSource[] voices;
    readonly AudioSource[] music = new AudioSource[2];
    readonly float[] targetGain = new float[2];
    readonly float[] initialGain = new float[2];
    readonly float[] currentGain = new float[2];
    float fadeAge;
    bool started, paused;
    int nextVoice;
#if UNITY_WEBGL && !UNITY_EDITOR
    // Web supports mixer gain but not native mixer filters. These are aligned,
    // prefiltered copies of the same compositions, rather than new tracks.
    readonly AudioSource[] webLowpass = new AudioSource[2];
    float webPauseBlend;
#endif
    public bool IsPaused => paused;
    public static float ToDecibels(float linear)
    { return linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Ensure()
    { if (Instance == null) new GameObject("SoundPass").AddComponent<SoundPass>(); }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        Bank = Resources.Load<SoundBank>("GameAudio/SoundBank");
        if (Bank == null || Bank.mixer == null) { Debug.LogError("SoundPass: missing sound bank or mixer."); enabled = false; return; }
        Bank.mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
        music[0] = Source("Menu music", Bank.musicGroup);
        music[1] = Source("Level music", Bank.musicGroup);
        music[0].clip = Bank.menuMusic; music[1].clip = Bank.levelMusic;
        foreach (AudioSource s in music) { s.loop = true; s.volume = 0f; s.priority = 0; }
#if UNITY_WEBGL && !UNITY_EDITOR
        for (int i = 0; i < 2; i++)
        {
            webLowpass[i] = Source("Web lowpass " + i, Bank.musicGroup);
            webLowpass[i].clip = i == 0 ? Bank.menuMusicLowpass : Bank.levelMusicLowpass;
            webLowpass[i].loop = true; webLowpass[i].volume = 0f; webLowpass[i].priority = 0;
        }
#endif
        voices = new AudioSource[Mathf.Clamp(Bank.voices, 1, 32)];
        for (int i = 0; i < voices.Length; i++) voices[i] = Source("SFX " + i, Bank.sfxGroup);
        SceneManager.sceneLoaded += SceneLoaded;
    }
    AudioSource Source(string label, AudioMixerGroup group)
    {
        var go = new GameObject(label); go.transform.SetParent(transform);
        AudioSource s = go.AddComponent<AudioSource>();
        s.playOnAwake = false; s.spatialBlend = 0f;
        s.ignoreListenerPause = true; s.outputAudioMixerGroup = group;
        return s;
    }
    void Start()
    {
        if (Bank == null) return;
        started = true; ApplyVolumes();
        (paused ? Bank.paused : Bank.normal).TransitionTo(0f);
        SelectMusic(SceneManager.GetActiveScene().name);
    }
    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        StopAllCoroutines(); SetPaused(false); SelectMusic(scene.name);
        if (scene.name == SceneNames.Win) Play(SoundEvent.Win);
    }
    void SelectMusic(string scene)
    {
        if (Bank == null || music[0] == null) return;
        int index = scene == SceneNames.MainMenu || scene == SceneNames.Win ? 0 : 1;
        // Same level track stays playing through campaign reloads.
        if (targetGain[index] == 1f) return;
        for (int i = 0; i < 2; i++) { initialGain[i] = currentGain[i]; targetGain[i] = i == index ? 1f : 0f; }
        if (!music[index].isPlaying)
        {
            music[index].Play();
#if UNITY_WEBGL && !UNITY_EDITOR
            webLowpass[index].Play(); webLowpass[index].time = music[index].time;
#endif
        }
        fadeAge = 0f;
    }
    void Update()
    {
        if (Bank == null) return;
        fadeAge += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(fadeAge / Bank.crossfadeSeconds);
        t = t * t * (3f - 2f * t);
#if UNITY_WEBGL && !UNITY_EDITOR
        webPauseBlend = Mathf.MoveTowards(webPauseBlend, paused ? 1f : 0f, Time.unscaledDeltaTime / Bank.snapshotSeconds);
        Bank.mixer.SetFloat("PauseDuckDb", Mathf.Lerp(0f, -12f, webPauseBlend));
#endif
        for (int i = 0; i < 2; i++)
        {
            currentGain[i] = Mathf.Lerp(initialGain[i], targetGain[i], t);
            music[i].volume = currentGain[i];
#if UNITY_WEBGL && !UNITY_EDITOR
            webLowpass[i].volume = music[i].volume * webPauseBlend;
            music[i].volume *= 1f - webPauseBlend;
            if (t >= 1f && targetGain[i] == 0f && webLowpass[i].isPlaying) webLowpass[i].Stop();
#endif
            if (t >= 1f && targetGain[i] == 0f && music[i].isPlaying) music[i].Stop();
        }
    }
    public void ApplyVolumes()
    {
        // Unity requires SetFloat in Start or later, rather than Awake.
        if (!started || Bank == null) return;
        Bank.mixer.SetFloat("MasterDb", Bank.masterDb);
        Bank.mixer.SetFloat("MusicDb", ToDecibels(SaveService.Data.musicVolume));
        Bank.mixer.SetFloat("SfxDb", ToDecibels(SaveService.Data.soundVolume));
    }
    public void SetPaused(bool value)
    {
        paused = value;
        if (!started || Bank == null) return;
        // Native never calls SetFloat on PauseDuck: the snapshot keeps control.
        (value ? Bank.paused : Bank.normal).TransitionTo(Bank.snapshotSeconds);
    }
    public void Play(SoundEvent id)
    {
        if (Bank == null) return;
        SoundCue cue = Bank.Find(id);
        if (cue != null) PlayClip(cue.clip, cue.gain, cue.varyPitch);
    }
    public void PlayClip(AudioClip clip, float gain = 1f, bool variation = true)
    {
        if (clip == null || voices == null) return;
        AudioSource voice = null;
        foreach (AudioSource candidate in voices) if (!candidate.isPlaying) { voice = candidate; break; }
        if (voice == null) { voice = voices[nextVoice++ % voices.Length]; voice.Stop(); }
        voice.clip = clip; voice.volume = Mathf.Clamp01(gain);
        voice.pitch = variation ? Random.Range(0.92f, 1.08f) : 1f;
        voice.Play();
    }
    public void PlayerDied() { Play(SoundEvent.Death); StartCoroutine(LoseAfterDeath()); }
    IEnumerator LoseAfterDeath() { yield return new WaitForSecondsRealtime(0.45f); Play(SoundEvent.Lose); }
    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded; Instance = null;
    }
}
