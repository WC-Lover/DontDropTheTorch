using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BreathSFXController : NetworkBehaviour
{

    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_breath_default;
    [SerializeField] AudioClip sfx_breath_heavy;
    [SerializeField] float DefaultVolume = 10f;

    public void SetVolumeValue(float volume = 0.01f)
    {
        if (volume >= 0.01f) sfx.volume = volume;
        Debug.Log($"sfx.volume breath -> {sfx.volume}");
    }

    public void BreathSFX()
    {
        Debug.Log("Default breath");
        sfx.clip = sfx_breath_default;
        sfx.Play();
    }

    public void HeavyBreathSFX()
    {
        Debug.Log("Heavy breath");
        sfx.clip = sfx_breath_heavy;
        sfx.Play();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }
}
