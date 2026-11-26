using UnityEngine;
// Serialized scene compatibility. SoundPass owns the two actual tracks.
[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour
{
    public static Music Instance { get; private set; }
    public AudioClip musicClip;
    public float volume;
    void Awake()
    {
        AudioSource legacy = GetComponent<AudioSource>();
        legacy.Stop(); legacy.playOnAwake = false; legacy.enabled = false;
        Instance = this; SoundPass.Ensure();
        if (SoundPass.Instance.Bank != null) legacy.outputAudioMixerGroup = SoundPass.Instance.Bank.musicGroup;
    }
    void OnDestroy() { if (Instance == this) Instance = null; }
}
