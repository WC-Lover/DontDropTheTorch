using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyController : NetworkBehaviour
{
    
    private NetworkObject networkObject;

    private EnemyAttributes attributes;
    private EnemySpawnAttributes spawnAttributes;

    private float health;
    private float speed;
    private float attackCooldown;

    private float nearestTargetCheckCooldown;
    private float nearestTargetCheckCooldownTime;
    private bool nearestTargetDead;

    private Rigidbody2D rigidBody;

    [SerializeField] AudioClip sfx_Spawn;
    [SerializeField] AudioClip sfx_Walk;
    [SerializeField] AudioClip sfx_Attack;

    public List<Transform> players;
    private Transform nearestPlayerTransform;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (attackCooldown <= 0 && collision.collider.TryGetComponent<HealthSystem>(out var healthSystem))
        {
            attackCooldown = attributes.AttackCooldown;
            var direction = collision.collider.transform.position - transform.position;
            Debug.Log(direction);
            if (healthSystem.TakeDamage(attributes.Damage, direction, attributes.AttackPushMultiplier))
            {
                // Unless player can be revived without trading zone it's fine to remove from list
                // as with trading zone enemies are removed
                players.Remove(nearestPlayerTransform);
                nearestPlayerTransform = FindNearestPlayer();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        networkObject = GetComponent<NetworkObject>();

        attributes = new EnemyAttributes();
        spawnAttributes = new EnemySpawnAttributes();

        health = attributes.Health;
        speed = attributes.Speed;
        attackCooldown = attributes.AttackCooldown;

        nearestTargetCheckCooldownTime = 2f;
        nearestTargetCheckCooldown = 0f;
        nearestTargetDead = false;

        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsServer) return;

        #region Find Nearest Player

        if (nearestTargetCheckCooldown <= 0 || nearestTargetDead)
        {
            nearestTargetCheckCooldown = nearestTargetCheckCooldownTime;
            nearestPlayerTransform = FindNearestPlayer();
        }
        else nearestTargetCheckCooldown -= Time.deltaTime;

        if (nearestPlayerTransform == null) return;

        #endregion

        var direction = nearestPlayerTransform.position - transform.position;

        if (attackCooldown > 0) attackCooldown -= Time.deltaTime;

        MoveTowardsNearestPlayer(direction);

    }

    private void MoveTowardsNearestPlayer(Vector3 direction)
    {
        var directionNormalized = direction.normalized;

        rigidBody.velocity = directionNormalized * speed;
    }

    private Transform FindNearestPlayer()
    {
        float nearestPlayerDistance = float.PositiveInfinity;
        Transform nearestPlayerTransform = null;
        for (int i = 0; i < players.Count; i++)
        {
            float distance = Mathf.Abs(Vector2.Distance(transform.position, players[i].position));
            // Smth there? if not dead, check how works in game
            if (distance < nearestPlayerDistance)
            {
                nearestPlayerDistance = distance;
                nearestPlayerTransform = players[i];
            }
        }
        return nearestPlayerTransform;
    }

    public void DealDamageToEnemyRpc(float damage)
    {
        if (damage >= health)
        {
            if (IsServer) DespawnEnemy();
            else DespawnEnemyRpc();
        }
    }

   [Rpc(SendTo.Server)]
    private void DespawnEnemyRpc()
    {
        DespawnEnemy();
    }

    public void DespawnEnemy()
    {
        networkObject.Despawn();
    }
}
