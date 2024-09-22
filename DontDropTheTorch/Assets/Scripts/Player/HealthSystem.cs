using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthSystem : NetworkBehaviour
{
    private HealthAttributes attributes;
    private float health;
    private Rigidbody2D rigidBody;
    private MovementSystem movementSystem;
    private EnemySpawnSystem enemySpawnSystem;
    [SerializeField] private float afterAttackEffect;
    public bool IsDead { private set; get; }

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        attributes = playerAttributes.HealthAttributes;

        health = attributes.HealthAmount;
    }

    public override void OnNetworkSpawn()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        movementSystem = GetComponent<MovementSystem>();
        enemySpawnSystem = GetComponent<EnemySpawnSystem>();
    }

    private void Update()
    {
        if (afterAttackEffect > 0)
        {
            afterAttackEffect -= Time.deltaTime;
            if (afterAttackEffect < 0)
            {
                rigidBody.velocity = Vector2.zero;
                if (!IsDead) movementSystem.enabled = true; 
            }
        }
    }

    public bool TakeDamage(float damage, Vector3 direction, float pushStrengthMultiplier)
    {
        PlayerIsDamaged(damage, direction, pushStrengthMultiplier);
        PlayerIsDamagedRpc(damage, direction, pushStrengthMultiplier);

        if (health <= 0)
        {
            PlayerIsDead();
            PlayerIsDeadRpc();
            return true;
        }

        return false;
    }

    [Rpc(SendTo.NotServer)]
    private void PlayerIsDeadRpc()
    {
        PlayerIsDead();
    }

    private void PlayerIsDead()
    {
        IsDead = true;
        if (IsOwner)
        {
            transform.position = new Vector3(-1000, -1000, 0);
            movementSystem.enabled = false;
            enemySpawnSystem.enabled = false;
            //gameObject.GetComponentInChildren<WeaponSystem>().enabled = false;
        }
        if (IsServer) TradingSystem.Instance.playersAlive--;
    }

    [Rpc(SendTo.NotServer)]
    private void PlayerIsDamagedRpc(float damage, Vector3 direction, float pushStrengthMultiplier)
    {
        PlayerIsDamaged(damage, direction, pushStrengthMultiplier);
    }

    private void PlayerIsDamaged(float damage, Vector2 direction, float pushStrengthMultiplier)
    {
        health -= damage;
        if (IsOwner)
        {
            movementSystem.enabled = false;
            afterAttackEffect = 0.5f;
            rigidBody.velocity = pushStrengthMultiplier * direction;
        }
        // Deal damage on all clients to show less HP or smth.
    }

    [Rpc(SendTo.NotServer)]
    public void RevivePlayerRpc(float tradingZoneAreaStartX)
    {
        RevivePlayer(tradingZoneAreaStartX);
    }

    public void RevivePlayer(float tradingZoneAreaStartX)
    {
        IsDead = false;
        health = attributes.HealthAmount;

        if (IsOwner)
        {
            transform.position = new Vector3(tradingZoneAreaStartX + 5, 0, 0);
            enemySpawnSystem.enabled = true;
            movementSystem.enabled = true;
            //gameObject.GetComponentInChildren<WeaponSystem>().enabled = true;
        }
        if (IsServer) RevivePlayerRpc(tradingZoneAreaStartX); 
    }

    public void UpdateHealth(float healthDiff)
    {
        health += healthDiff;

        // Update HealthBar
    }
}
