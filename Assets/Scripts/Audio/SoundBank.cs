using UnityEngine;
using UnityEngine.Audio;
public enum SoundEvent
{
    Jump, Hit, Pickup, Heal, PlayerShot, PatrollerWindUp, ShooterWindUp,
    ChargerWindUp, BossFanWindUp, BossRainWindUp, EnemyShot, EnemyStrike,
    EnemyHit, EnemyDeath, Death, UIClick, Win, Lose, LevelClear
}
[System.Serializable]
public sealed class SoundCue
{
    public SoundEvent id;
    public AudioClip clip;
    [Range(0f, 1f)] public float gain = 0.7f;
    public bool varyPitch = true;
}
[CreateAssetMenu(menuName = "Coinbound/Audio/Sound bank")]
public sealed class SoundBank : ScriptableObject
{
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup, sfxGroup;
    public AudioMixerSnapshot normal, paused;
    public AudioClip menuMusic, levelMusic;
    public AudioClip menuMusicLowpass, levelMusicLowpass;
    [Min(0.1f)] public float crossfadeSeconds = 1f;
    [Min(0.01f)] public float snapshotSeconds = 0.25f;
    [Range(-80f, 0f)] public float masterDb = -3f;
    [Range(1, 32)] public int voices = 16;
    public SoundCue[] cues;
    public SoundCue Find(SoundEvent id)
    {
        foreach (SoundCue cue in cues) if (cue.id == id) return cue;
        return null;
    }
}
