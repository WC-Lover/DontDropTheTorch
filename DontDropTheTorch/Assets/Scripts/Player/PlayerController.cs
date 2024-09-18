using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    PlayerAttributes playerAttributes;
    HealthSystem healthSystem;
    MovementSystem movementSystem;
    WeaponSystem weaponSystem;
    EnemySpawnSystem enemySpawnSystem;
    TradingSystem tradingSystem;
    [SerializeField] GameObject weapon;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerAttributes = new PlayerAttributes();

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

        CameraFollowPlayer.PlayerToFollowTransform = transform;
    }
}
