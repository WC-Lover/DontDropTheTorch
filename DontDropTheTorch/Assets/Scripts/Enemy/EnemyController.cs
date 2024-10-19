using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class EnemyController : NetworkBehaviour
{
    
    public int ChaseMode = 0;


    private NetworkVariable<float> netHealth = new NetworkVariable<float>();

    private NetworkObject networkObject;

    private EnemyAttributes attributes;

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
    private Rigidbody2D nearestPlayerRB;

    private float predictionTime = 1f;
    private float flankDistance = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        TryAttackNearbyCollider(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsServer) return;

        TryAttackNearbyCollider(collision);
    }

    private void TryAttackNearbyCollider(Collision2D collision)
    {
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

        health = attributes.Health;
        if (IsServer) netHealth.Value = attributes.Health;
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

        //if (ChaseMode == 0)
        //{

        //#region Wondering with restricted eye sight
        ///*
        // * Zombies are wondering around with radius of what they see and chase
        // */

        //float enemieSeeRadius = 4.5f; // range of detection
        //float leapRange = attributes.LeapRange; // range of active chase phase
        //float attackRange = attributes.AttackRange; // range of attack




        //#endregion

        //}
        //else if (ChaseMode == 1)
        //{

        //}

        #region Find Nearest Player

        if (nearestTargetCheckCooldown <= 0 || nearestTargetDead)
        {
            nearestTargetCheckCooldown = nearestTargetCheckCooldownTime;
            nearestPlayerTransform = FindNearestPlayer();
        }
        else nearestTargetCheckCooldown -= Time.deltaTime;

        if (nearestPlayerTransform == null) return;

        #endregion

        // if distance from enemy to player is shorter than leap distance -> charge directly into player.

        if (Vector2.Distance(transform.position, nearestPlayerTransform.position) <= attributes.LeapRange)
        {
            //MoveTowardsNearestPlayer();
        }

        // predict players' next position

        var playerMoveDirection = PredictPlayerPosition();

        var flank = AddFlank(playerMoveDirection);

        //Debug.DrawLine(transform.position, playerMoveDirection, Color.white);
        //Debug.DrawLine(transform.position, flank, Color.black);

        var direction = flank - transform.position;
        if (attackCooldown > 0) attackCooldown -= Time.deltaTime;

        MoveTowardsNearestPlayer(direction);

    }

    Vector3 PredictPlayerPosition()
    {
        if (nearestPlayerRB != null)
        {
            Vector2 playerVelocity = nearestPlayerRB.velocity;

            Vector2 predictedPosition = (Vector2)nearestPlayerTransform.position + playerVelocity * predictionTime;

            return predictedPosition;
        }

        return nearestPlayerTransform.position;
    }

    Vector3 AddFlank(Vector2 predictedPosition)
    {
        Vector2 directionToPlayer = (predictedPosition - (Vector2)transform.position).normalized;

        Vector2 perpendicularDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x);

        Vector2 flankedPosition = predictedPosition + perpendicularDirection * flankDistance;

        return flankedPosition;
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
        if (!nearestPlayerTransform) return null;

        nearestPlayerRB = nearestPlayerTransform.GetComponent<Rigidbody2D>();

        return nearestPlayerTransform;
    }

    // add same to TradingPoints? instead of adding there?
    public void DealDamageToEnemy(float damage, Vector2 rayDirection)
    {
        if (netHealth.Value - damage <= 0) TradingSystem.Instance.tradingPoints++;

        DealDamageToEnemyRpc(damage, rayDirection);
    }

    [Rpc(SendTo.Server)]
    private void DealDamageToEnemyRpc(float damage, Vector2 rayDirection)
    {
        netHealth.Value -= damage;
        //healthBarImage.fillAmount = Mathf.Clamp(attributes.Health / health, 0, 1);
    }

    private void HealthChanged(float previousValue, float newValue)
    {
        if (!IsServer) return;

        health = newValue;

        if (health <= 0) DespawnEnemy();

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
