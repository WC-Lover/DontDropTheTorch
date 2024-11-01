using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSFXSystem : MonoBehaviour
{
    public static BackgroundSFXSystem Instance;

    BackgroundNoicesSFXController backgroundNoicesSFX;
    BackgroundSFXController backgroundSFX;

    void Start()
    {
        Instance = this;

        backgroundNoicesSFX = GetComponentInChildren<BackgroundNoicesSFXController>();
        backgroundNoicesSFX.SetVolumeValue();

        backgroundSFX = GetComponentInChildren<BackgroundSFXController>();
        backgroundSFX.SetVolumeValue();

        backgroundSFX.MainMenuSFX();
    }

    void Update()
    {
        
    }

    public void CrowsSFX()
    {
        backgroundNoicesSFX.CrowsSFX();
    }

    public void StartInGameBackgroundSFX()
    {
        backgroundSFX.BackgroundSFX();
    }

    public void StartInGameBackgroundRainSFX()
    {
        backgroundSFX.RainSFX();
    }

    public void StopBackgroundSFX()
    {
        backgroundSFX.StopSFX();
    }
}
