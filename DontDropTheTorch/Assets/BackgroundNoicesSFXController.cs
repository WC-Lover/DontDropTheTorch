using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundNoicesSFXController : MonoBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_crows;
    [SerializeField] AudioClip sfx_loading_players;
    [SerializeField] AudioClip sfx_game_background;

    public void SetVolumeValue(float value = 0.01f)
    {
        sfx.volume = value;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CrowsSFX()
    {
        sfx.clip = sfx_crows;
        sfx.Play();
    }
}
