using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class Assignment12Tools
{
    static void Check(bool pass, string description)
    {
        if (!pass) throw new InvalidOperationException("Sound Pass: " + description);
        Debug.Log("PASS " + description);
    }
    [MenuItem("Tools/Assignment 12/1. Check Audio Assets and Save")]
    public static void ValidateAssets()
    {
        SoundBank bank = Resources.Load<SoundBank>("GameAudio/SoundBank");
        Check(bank != null && bank.mixer != null, "SoundBank and imported AudioMixer");
        Check(bank.musicGroup != null && bank.sfxGroup != null, "AudioSource routing references");
        foreach (string name in new[] { "Master", "Music", "SFX", "PauseDuck" })
            Check(bank.mixer.FindMatchingGroups(name).Any(x => x.name == name), "Mixer group " + name);
        Check(bank.mixer.FindSnapshot("Normal") == bank.normal && bank.normal != null, "Normal snapshot");
        Check(bank.mixer.FindSnapshot("Paused") == bank.paused && bank.paused != null, "Paused snapshot");
        foreach (string name in new[] { "MasterDb", "MusicDb", "SfxDb", "PauseDuckDb" })
            Check(bank.mixer.GetFloat(name, out float value), "Exposed parameter " + name);
        Check(bank.menuMusic != null && bank.levelMusic != null && bank.menuMusic != bank.levelMusic, "Two distinct tracks");
        Check(bank.menuMusicLowpass != null && bank.levelMusicLowpass != null, "Web lowpass copies");
        Check(bank.menuMusic.samples == bank.menuMusicLowpass.samples && bank.levelMusic.samples == bank.levelMusicLowpass.samples, "Normal and filtered loops aligned");
        Check(bank.crossfadeSeconds == 1f, "One-second crossfade");
        Check(bank.cues.Length == Enum.GetValues(typeof(SoundEvent)).Length, "All 19 events configured");
        Check(bank.cues.Select(x => x.id).Distinct().Count() == bank.cues.Length, "Unique sound event IDs");
        foreach (SoundCue cue in bank.cues)
            Check(cue.clip != null && cue.clip.length > 0f && cue.gain > 0f, "Clip/gain " + cue.id);
        Check(SoundPass.ToDecibels(0f) == -80f && SoundPass.ToDecibels(1f) == 0f &&
            Mathf.Abs(SoundPass.ToDecibels(.5f) + 6.0206f) < .001f, "Log10 x 20, including safe mute");
        string folder = Path.Combine(Path.GetTempPath(), "CoinboundSoundTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            SaveRepository repository = new SaveRepository(folder);
            SaveData save = SaveService.Defaults(); save.musicVolume = .37f; save.soundVolume = .61f;
            Check(repository.Write(save), "Write audio settings to isolated JSON");
            SaveData restored = new SaveRepository(folder).Load(SaveService.Defaults, SaveService.Validate);
            Check(Mathf.Approximately(restored.musicVolume, .37f) && Mathf.Approximately(restored.soundVolume, .61f), "New repository instance restores both volumes");
            save.musicVolume = 0f; save.soundVolume = 1f; repository.Write(save);
            restored = new SaveRepository(folder).Load(SaveService.Defaults, SaveService.Validate);
            Check(restored.musicVolume == 0f && restored.soundVolume == 1f, "Mute/full settings survive reload");
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
        Debug.Log("Audio asset checks passed. Perform the ears, pause, restart and phone tests from Docs/Assignment12/START_RU.md.");
    }
    [MenuItem("Tools/Assignment 12/2. Open Mixer")]
    static void OpenMixer()
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Resources/GameAudio/Coinbound.mixer");
        Selection.activeObject = mixer; AssetDatabase.OpenAsset(mixer);
        EditorApplication.ExecuteMenuItem("Window/Audio/Audio Mixer");
    }
    [MenuItem("Tools/Assignment 12/3. Audit Live Audio Routing")]
    static void AuditLive()
    {
        Check(Application.isPlaying, "Enter Play Mode first");
        Check(UnityEngine.Object.FindObjectsByType<SoundPass>(FindObjectsSortMode.None).Length == 1, "Exactly one persistent SoundPass");
        AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource s in sources) Check(s.outputAudioMixerGroup != null, "Routed source: " + s.gameObject.name);
        Check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1, "Exactly one listener");
        Debug.Log("Live audio sources: " + sources.Length + ". Check sounding mix by ear; routing alone does not prove it sounds correct.");
    }
    [MenuItem("Tools/Assignment 12/Sound Audition")]
    static void Audition() { EditorWindow.GetWindow<SoundAuditionWindow>("Sound Audition"); }
}
public sealed class SoundAuditionWindow : EditorWindow
{
    Vector2 scroll;
    void OnGUI()
    {
        EditorGUILayout.HelpBox("Enter Play Mode. These buttons audition assets; record real gameplay for Canvas proof.", MessageType.Info);
        GUI.enabled = Application.isPlaying;
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (SoundEvent id in Enum.GetValues(typeof(SoundEvent)))
            if (GUILayout.Button(id.ToString(), GUILayout.Height(28))) Sfx.Play(id);
        EditorGUILayout.EndScrollView(); GUI.enabled = true;
    }
}
