using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSFXController : MonoBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_coin_drop;
    [SerializeField] AudioClip sfx_coin_collect;

    public void CoinDropSFX()
    {
        sfx.clip = sfx_coin_drop;
        sfx.Play();
    }

    internal void CoinCollectSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
        sfx.clip = sfx_coin_collect;
        sfx.Play();
    }
}
