using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeatController : MonoBehaviour
{
    private HeartBeatSFXController heartBeatSFXController;
    private HealthSystem healthSystem;
    private MovementSystem movementSystem;

    private float prevStressLevel;
    private float prevStaminaLevel;
    private bool heartBeating;

    void Start()
    {
        heartBeatSFXController = GetComponent<HeartBeatSFXController>();
        heartBeatSFXController.SetVolumeValue();
        
        healthSystem = GetComponentInParent<HealthSystem>();
        prevStressLevel = healthSystem.stress;

        movementSystem = GetComponentInParent<MovementSystem>();
        prevStaminaLevel = movementSystem.stamina;

        heartBeating = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSystem.stress < 10 && movementSystem.stamina > 90) return;

        if (!heartBeating && (healthSystem.stress >= 20 || movementSystem.stamina <= 80))
        {
            heartBeating = true;
            heartBeatSFXController.HeartBeatSFX();
        }

        if (prevStressLevel - 2.5 > healthSystem.stress || healthSystem.stress > 2.5 + prevStressLevel)
        {
            prevStressLevel = healthSystem.stress;
            heartBeatSFXController.SetVolumeValue(healthSystem.stress / 100f);
        }
        else if (prevStaminaLevel - 2.5 > movementSystem.stamina || movementSystem.stamina > prevStaminaLevel + 2.5)
        {
            prevStaminaLevel = movementSystem.stamina;
            heartBeatSFXController.SetVolumeValue((100 - movementSystem.stamina) / 100f);
        }
    }
}
