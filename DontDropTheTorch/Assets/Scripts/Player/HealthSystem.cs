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
    }

    private void Update()
    {
        if (afterAttackEffect > 0)
        {
            afterAttackEffect -= Time.deltaTime;
            if (afterAttackEffect < 0) movementSystem.enabled = true; 
        }
    }

    public bool TakeDamage(float damage, Vector3 direction, float pushStrengthMultiplier)
    {
        PlayerIsDamaged(damage, direction, pushStrengthMultiplier);
        PlayerIsDamagedRpc(damage, direction, pushStrengthMultiplier);

        if (health <= 0)
        {
            PlayerIsDead();
            return true;
        }

        return false;
    }

    [Rpc(SendTo.NotServer)]
    private void PlayerIsDamagedRpc(float damage, Vector3 direction, float pushStrengthMultiplier)
    {
        PlayerIsDamaged(damage, direction, pushStrengthMultiplier);
    }

    private void PlayerIsDead()
    {
        IsDead = true;
    }

    private void PlayerIsDamaged(float damage, Vector2 direction, float pushStrengthMultiplier)
    {
        if (IsOwner)
        {
            movementSystem.enabled = false;
            afterAttackEffect = 0.25f;
            rigidBody.velocity = pushStrengthMultiplier * direction;
        }
        // Deal damage on all clients to show less HP or smth.
    }

    public void RevivePlayer()
    {
        Debug.Log("revived");
        gameObject.SetActive(true);
    }

    public void UpdateHealth(float healthDiff)
    {
        health += healthDiff;

        // Update HealthBar
    }
}
