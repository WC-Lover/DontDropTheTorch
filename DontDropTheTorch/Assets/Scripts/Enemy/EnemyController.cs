using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class EnemyController : NetworkBehaviour
{

    private NetworkVariable<float> netHealth = new NetworkVariable<float>();

    private NetworkObject networkObject;

    private EnemyAttributes attributes;
    private EnemySpawnAttributes spawnAttributes;

    [SerializeField] private Image healthBarImage;
    private float health;
    private float speed;
    private float attackCooldown;

    private float nearestTargetCheckCooldown;
    private float nearestTargetCheckCooldownTime;
    private bool nearestTargetDead;

    private Rigidbody2D rigidBody;

    private EnemySFXController enemySFXController;

    private List<Transform> players;
    private Transform nearestPlayerTransform;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (attackCooldown <= 0 && collision.collider.TryGetComponent<HealthSystem>(out var healthSystem))
        {
            attackCooldown = attributes.AttackCooldown;
            var direction = collision.collider.transform.position - transform.position;

            enemySFXController.AttackSFX();

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
        enemySFXController = GetComponent<EnemySFXController>();

        netHealth.OnValueChanged += HealthChanged;

        players = new List<Transform>(LobbyManager.LobbyPlayersTransformsInludingLocal);
        
        attributes = new EnemyAttributes();
        spawnAttributes = new EnemySpawnAttributes();

        health = attributes.Health;
        netHealth.Value = attributes.Health;
        speed = attributes.Speed;
        attackCooldown = attributes.AttackCooldown;

        nearestTargetCheckCooldownTime = 2f;
        nearestTargetCheckCooldown = 0f;
        nearestTargetDead = false;

        rigidBody = GetComponent<Rigidbody2D>();

        enemySFXController.SpawnSFX();
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
        enemySFXController.WalkSFX();

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

    // Make rpc, no Client can deal damage to Enemy on it's side, only through Host!
    public void DealDamageToEnemyRpc(float damage, Vector2 rayDirection)
    {
        netHealth.Value -= damage;
        healthBarImage.fillAmount = Mathf.Clamp(attributes.Health / health, 0, 1);
    }

    private void HealthChanged(float previousValue, float newValue)
    {
        if (!IsServer) return;

        health = newValue;

        if (health <= 0)
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
        EnemySpawnSystem.Instance.enemyControllers.Remove(this);
        networkObject.Despawn();
    }
}
