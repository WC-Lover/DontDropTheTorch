using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class WeaponSFXController : NetworkBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_fire;
    [SerializeField] AudioClip sfx_reload;
    [SerializeField] AudioClip sfx_emptyAmmoShot;

    public void EmptyAmmoShotSFX()
    {
        sfx.clip = sfx_emptyAmmoShot;
        sfx.Play();
    }

    public void ShootSFX()
    {
        sfx.clip = sfx_fire;
        sfx.Play();

        if (IsOwner) ShootSFXRpc();
    }

    [Rpc(SendTo.NotOwner)]
    private void ShootSFXRpc()
    {
        ShootSFX();
    }

    public void ReloadSFX()
    {
        sfx.clip = sfx_reload;
        sfx.Play();

        if (IsOwner) ReloadSFXRpc(); 
    }

    [Rpc(SendTo.NotOwner)]
    private void ReloadSFXRpc()
    {
        ReloadSFX();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }

}
