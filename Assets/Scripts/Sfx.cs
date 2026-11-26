using UnityEngine;
// Legacy clips and new events share the routed SFX voice pool.
public static class Sfx
{
    public static void Play(AudioClip clip, float volume = 1f)
    { SoundPass.Ensure(); SoundPass.Instance.PlayClip(clip, volume); }
    public static void Play(SoundEvent id)
    { SoundPass.Ensure(); SoundPass.Instance.Play(id); }
}
