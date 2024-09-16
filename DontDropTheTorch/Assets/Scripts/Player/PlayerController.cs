using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    PlayerAttributes playerAttributes;
    HealthSystem healthSystem;
    MovementSystem movementSystem;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerAttributes = new PlayerAttributes();

        healthSystem = GetComponent<HealthSystem>();
        healthSystem.SetAttributes(playerAttributes);

        movementSystem = GetComponent<MovementSystem>();
        movementSystem.SetAttributes(playerAttributes);
    }
}
