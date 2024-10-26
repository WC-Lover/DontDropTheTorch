using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HeartBeatSFXController : NetworkBehaviour
{

    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_heart_beat_default;
    [SerializeField] AudioClip sfx_heart_beat_50_stress_stamina;
    [SerializeField] AudioClip sfx_heart_beat_heavy;

    public void SetVolumeValue(float volume = 0.01f)
    {
        if (volume >= 0.01f) sfx.volume = volume;
        Debug.Log($"sfx.volume heart -> {sfx.volume}");
    }

    public void HeartBeatSFX()
    {
        Debug.Log("Heart beat");
        sfx.clip = sfx_heart_beat_default;
        sfx.Play();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }
}
