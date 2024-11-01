using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySFXController : NetworkBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_spawn;
    [SerializeField] AudioClip sfx_walk;
    [SerializeField] AudioClip sfx_attack;

    public void SetVolumeValue(float value = 0.01f)
    {
        sfx.volume = value;
    }

    public void SpawnSFX()
    {
        sfx.clip = sfx_spawn;
        sfx.Play();

        if (IsOwner) SpawnSFXRpc();
    }

    [Rpc(SendTo.NotOwner)]
    private void SpawnSFXRpc()
    {
        SpawnSFX();
    }

    public void WalkSFX()
    {
        if (sfx.isPlaying) return;

        sfx.clip = sfx_walk;
        sfx.Play();

        if (IsOwner) WalkSFXRpc();
    }

    [Rpc(SendTo.NotOwner)]
    private void WalkSFXRpc()
    {
        WalkSFX();
    }

    public void AttackSFX()
    {
        // Not added

        //sfx.clip = sfx_attack;
        //sfx.Play();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }

    internal void LeapSFX()
    {
        // play leap sfx
    }
}
