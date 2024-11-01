using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSFXController : MonoBehaviour
{
    [SerializeField] AudioSource sfx;

    [SerializeField] AudioClip sfx_main_menu;
    [SerializeField] AudioClip sfx_loading_players;
    [SerializeField] AudioClip sfx_game_background;
    [SerializeField] AudioClip sfx_rain;

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

    public void MainMenuSFX()
    {
        sfx.clip = sfx_main_menu;
        sfx.Play();
    }

    public void BackgroundSFX()
    {

    }

    public void RainSFX()
    {
        sfx.clip = sfx_rain;
        sfx.Play();
    }

    public void StopSFX()
    {
        if (sfx.isPlaying) sfx.Stop();
    }
}
