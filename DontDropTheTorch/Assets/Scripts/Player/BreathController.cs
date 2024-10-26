using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathController : MonoBehaviour
{
    private BreathSFXController breathSFXController;
    private HealthSystem healthSystem;
    private MovementSystem movementSystem;

    private bool heavyBreathing;
    private bool breathing;
    private float prevStressLevel;
    private float prevStaminaLevel;

    void Start()
    {
        breathSFXController = GetComponent<BreathSFXController>();
        breathSFXController.SetVolumeValue();

        healthSystem = GetComponent<HealthSystem>();
        movementSystem = GetComponent<MovementSystem>();

        prevStaminaLevel = movementSystem.stamina;
        prevStressLevel = healthSystem.stress;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSystem.stress < 10 && movementSystem.stamina > 90) return;

        if (!breathing && !heavyBreathing && (healthSystem.stress >= 20 || movementSystem.stamina <= 80))
        {
            breathing = true;
            breathSFXController.BreathSFX();
        }

        if (!heavyBreathing && (healthSystem.stress >= 50 || movementSystem.stamina <= 50))
        {
            heavyBreathing = true;
            breathSFXController.HeavyBreathSFX();
        }

        if (prevStressLevel - 2.5 > healthSystem.stress || healthSystem.stress > 2.5 + prevStressLevel)
        {
            prevStressLevel = healthSystem.stress;
            breathSFXController.SetVolumeValue(healthSystem.stress / 100f);
        }
        else if (prevStaminaLevel - 2.5 > movementSystem.stamina || movementSystem.stamina > prevStaminaLevel + 2.5)
        {
            prevStaminaLevel = movementSystem.stamina;
            breathSFXController.SetVolumeValue((100 - movementSystem.stamina) / 100f);
        }
    }
}
