using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    // In instance create a List of PlayerControllers?
    // Or use LobbyManager.LobbyPlayersTransformsInludingLocal when update enemies attributes after trader
    PlayerAttributes playerAttributes;
    HealthSystem healthSystem;
    MovementSystem movementSystem;
    WeaponSystem weaponSystem;
    EnemySpawnSystem enemySpawnSystem;
    TradingSystem tradingSystem;
    [SerializeField] GameObject weapon;

    public override void OnNetworkSpawn()
    {
        playerAttributes = new PlayerAttributes();

        if (IsOwner)
        {
            CameraShakeManager.instance.virtualCamera.Follow = transform;
        }
        else
        {
            LobbyManager.LobbyPlayersTransforms.Add(transform);
        }

        LobbyManager.LobbyPlayersTransformsInludingLocal.Add(transform);

        healthSystem = GetComponent<HealthSystem>();
        healthSystem.SetAttributes(playerAttributes);

        movementSystem = GetComponent<MovementSystem>();
        movementSystem.SetAttributes(playerAttributes);

        weaponSystem = weapon.GetComponent<WeaponSystem>();
        weaponSystem.SetAttributes(playerAttributes);

        enemySpawnSystem = GetComponent<EnemySpawnSystem>();
        enemySpawnSystem.SetAttributes(playerAttributes);

        tradingSystem = GetComponent<TradingSystem>();
        tradingSystem.SetAttributes(playerAttributes);

    }
}
