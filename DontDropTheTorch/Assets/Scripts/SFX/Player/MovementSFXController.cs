using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MovementSFXController : NetworkBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_wet_run;
    [SerializeField] AudioClip sfx_dash;
    [SerializeField] AudioClip sfx_boost;

    public void DashSFX()
    {
    }

    public void BoostSFX()
    {
    }

    public void RunSFX()
    {
        if (sfx.isPlaying) return;

        sfx.clip = sfx_wet_run;
        sfx.Play();

        // prbbly switch to NetworkVariable ushort? if variable has switched use switch case?
        if (IsOwner) RunSFXRpc();
    }

    private void RunSFXRpc()
    {
        RunSFX();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }
}
