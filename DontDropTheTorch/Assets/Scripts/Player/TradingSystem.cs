using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


/*
 * Player can sell whatever was bought during current/prev trading period.
 * If on prev trading, +1 damage was bought then it can be sold during current trading.
 * If on prev-prev trading, +2 damage was bought and on prev +3 damage was bought, then on current trading only +3 damage can be sold from prev trading by half price?
 * Allow mode where you can sell and buy anything and everything what was bought previously.
 * Maybe add system where you can trade with your teammates to exchange skills, you set the price of your own choice.(or Common<->Common, Rare<->Rare equal exchange)
 */
public class TradingSystem : NetworkBehaviour
{

    public static TradingSystem Instance;
    HealthSystem healthSystem;

    [SerializeField] private GameObject traderPrefab;

    PlayerAttributes playerAttributes;
    private float traderSpawnTime;
    private float tradingTime;

    private bool isTrading;
    private bool isInsideTradingZone;
    private bool isTraderSpawned;
    private float tradingZoneAreaStartX;
    private NetworkObject traderNetworkObject;
    private int playersInsideTradingZone;

    public int playersAlive;
    public int tradingPoints; // make NetworkVariable and update on server detects kill!

    private List<Transform> otherPlayers;

    private float dmgPercent;
    private float projectileAngleSpreadPercent;
    private float rangePercent;
    private float critPercent;
    private float critChancePercent;
    private float fireRatePercent;
    private float reloadTimePercent;

    private float healthPercent;
    private float regenPercent;
    private float healthRegenerationCooldownPercent;
    private float fearAmountPercent;

    private float staminaPercent;
    private float staminaRegenPercent;
    private float moveSpeedPercent;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        this.playerAttributes = playerAttributes;

        traderSpawnTime = playerAttributes.TradingAttributes.TraderSpawnTime;
        tradingTime = playerAttributes.TradingAttributes.TradingTime;

        isTrading = false;
        isInsideTradingZone = false;
        isTraderSpawned = false;

        playersInsideTradingZone = 0;
        tradingPoints = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (IsOwner && !IsServer) AddPlayerAliveRpc();

        Instance = this;
        otherPlayers = LobbyManager.LobbyPlayersTransforms;
        healthSystem = GetComponent<HealthSystem>();
        playersAlive = 1;
    }

    private void Update()
    {
        // if Host dies, trader won't be spawned
        // IsServer check is enough? anybody from servers' side can spawn trader?
        // IsOwnedByServer maybe is not IsServer?
        if (!IsOwner) return;

        #region Spawn Trader

        if (IsServer && !isTraderSpawned)
        {
            if (traderSpawnTime > 0) traderSpawnTime -= Time.deltaTime;
            else
            {
                traderSpawnTime = playerAttributes.TradingAttributes.TraderSpawnTime;

                float mostRightPlayerX = GetMostRightPlayerX();

                SpawnTrader(mostRightPlayerX, 1); // change 1 to mostRightPlayerLightBeamDistance + 5.5, lengthTradingZone/2 = 5, TradingWallThickness/2 = 0.5
                SpawnTraderRpc(mostRightPlayerX, 1);
            }
        }

        #endregion

        #region Trading process

        if (isTraderSpawned)
        {
            if (!isInsideTradingZone && HasPlayerEnteredTradingZone())
            {
                isInsideTradingZone = true;
                if (IsServer) playersInsideTradingZone++;
                else PlayerHasEnteredTradingZoneRpc();
            }

            if (IsServer && !isTrading && playersInsideTradingZone == playersAlive)
            {
                ReviveDeadPlayers();
                StartTrading();
                StartTradingRpc();
            }

            if (IsServer && isTrading)
            {
                tradingTime -= Time.deltaTime;
                if (tradingTime <= 0)
                {
                    DisableTradingZone();
                    DisableTradingZoneRpc();
                }
            }
        }

        #endregion

    }

    [Rpc(SendTo.NotServer)]
    private void DisableTradingZoneRpc()
    {
        DisableTradingZone();
    }

    private void DisableTradingZone()
    {
        Instance.isTrading = false;
        Instance.isTraderSpawned = false;
        Instance.isInsideTradingZone = false;
        Instance.playersInsideTradingZone = 0;
        EnemySpawnSystem.Instance.despawnAllEnemies = false;
        EnemySpawnSystem.Instance.disableEnemySpawn = false;
        GetComponentInChildren<WeaponSystem>().enabled = true;
        if (IsServer) traderNetworkObject.Despawn();
    }

    [Rpc(SendTo.NotServer)]
    private void StartTradingRpc()
    {
        StartTrading();
    }

    private void StartTrading()
    {
        Instance.isTrading = true;
        Instance.tradingTime = playerAttributes.TradingAttributes.TradingTime;
        EnemySpawnSystem.Instance.despawnAllEnemies = true;
        EnemySpawnSystem.Instance.disableEnemySpawn = true;
        GetComponentInChildren<WeaponSystem>().enabled = false;

        #region Damage

        Instance.dmgPercent = Random.Range(10, 20) / 100f;
        Instance.projectileAngleSpreadPercent = Random.Range(10, 20) / 100f;
        Instance.rangePercent = Random.Range(10, 20) / 100f;
        Instance.critPercent = Random.Range(10, 20) / 100f;
        Instance.critChancePercent = Random.Range(10, 20) / 100f;
        Instance.fireRatePercent = Random.Range(10, 20) / 100f;
        Instance.reloadTimePercent = Random.Range(10, 20) / 100f;

        #endregion

        #region Health

        Instance.healthPercent = Random.Range(2, 5) / 100f;
        Instance.regenPercent = Random.Range(2, 5) / 100f;

        #endregion
    }

    private void ReviveDeadPlayers()
    {
        otherPlayers.Add(transform);
        foreach (Transform player in otherPlayers)
        {
            var deadPlayer = player.GetComponent<HealthSystem>();
            if (deadPlayer.IsDead)
            {
                playersAlive++;
                deadPlayer.RevivePlayer(tradingZoneAreaStartX);
            }
        }
        otherPlayers.Remove(transform);
    }

    [Rpc(SendTo.Server)]
    private void PlayerHasEnteredTradingZoneRpc()
    {
        Instance.playersInsideTradingZone++;
    }

    private bool HasPlayerEnteredTradingZone()
    {
        // Rework as Y axis can change during the game
        return transform.position.x >= tradingZoneAreaStartX &&
            transform.position.y >= -5 &&
            transform.position.y <= 5;
    }

    private float GetMostRightPlayerX()
    {
        float mostRightPlayerX = transform.position.x; // Host position

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            float playerX = otherPlayers[i].position.x;

            if (mostRightPlayerX < playerX) mostRightPlayerX = playerX;
        }

        return mostRightPlayerX;
    }

    [Rpc(SendTo.NotServer)]
    private void SpawnTraderRpc(float mostRightPlayerX, float mostRightPlayerBeamDistance)
    {
        SpawnTrader(mostRightPlayerX, mostRightPlayerBeamDistance);
    }

    private void SpawnTrader(float mostRightPlayerX, float mostRightPlayerBeamDistance)
    {
        if (IsServer)
        {
            GameObject trader = Instantiate(traderPrefab);
            trader.transform.position = new Vector2(mostRightPlayerX + mostRightPlayerBeamDistance + 16, 0); // change +2 to the length of the light beam
            traderNetworkObject = trader.GetComponent<NetworkObject>();
            traderNetworkObject.Spawn();
        }
        
        Instance.isTraderSpawned = true;
        // 0.5f is wall width, player needs to be inside zone, not in wall
        // lengthOfTheTradingArea/2 = 5
        Instance.tradingZoneAreaStartX = mostRightPlayerX + mostRightPlayerBeamDistance + 0.5f - 5 + 16;
    }

    [Rpc(SendTo.Server)]
    private void AddPlayerAliveRpc()
    {
        Instance.playersAlive++;
    }

    void OnGUI()
    {
        if (IsOwner && isTrading) //  && tradingPoints > 0
        {

            #region Weapon

            GUILayout.BeginArea(new Rect(0, 0, 200, 200));

            GUILayout.TextArea("WEAPON");

            if (GUILayout.Button($"Damage + {dmgPercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.Damage *= 1 + dmgPercent;
                dmgPercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Projectile Amount + 1")) // Rare // Can't update if not enough bulets?
            {
                playerAttributes.WeaponAttributes.ProjectileAmount++;
                tradingPoints--;
            }

            if (GUILayout.Button($"Projectile Angle - {projectileAngleSpreadPercent}%")) // Common
            {
                playerAttributes.WeaponAttributes.ProjectileSpreadAngle *= 1 - projectileAngleSpreadPercent;
                projectileAngleSpreadPercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Penetration + 1")) // Rare
            {
                playerAttributes.WeaponAttributes.Penetration++;
                tradingPoints--;
            }

            if (GUILayout.Button($"Range + {rangePercent}%")) // Common
            {
                playerAttributes.WeaponAttributes.Range *= 1 + rangePercent;
                rangePercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Crit + {critPercent}%")) // Rare
            {
                playerAttributes.WeaponAttributes.Crit *= 1 + critPercent;
                critPercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Crit chance + {critChancePercent}%")) // Rare
            {
                playerAttributes.WeaponAttributes.CritChance *= 1 + critChancePercent;
                critChancePercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Fire rate + {fireRatePercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.FireRate *= 1 - fireRatePercent;
                fireRatePercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Reload time - {reloadTimePercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.ReloadTime *= 1 - reloadTimePercent;
                reloadTimePercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            // BulletAmount?

            GUILayout.EndArea();

            #endregion

            #region Health

            GUILayout.BeginArea(new Rect(200, 0, 200, 200));

            GUILayout.TextArea("HEALTH");

            if (GUILayout.Button($"Amount + {healthPercent}%")) // Uncommon
            {
                var healthBeforeUpdate = playerAttributes.HealthAttributes.HealthAmount;
                playerAttributes.HealthAttributes.HealthAmount *= 1 + healthPercent;
                var healthDiff = Mathf.RoundToInt(playerAttributes.HealthAttributes.HealthAmount - healthBeforeUpdate);
                healthPercent = Random.Range(2, 5) / 100f;
                healthSystem.UpdateHealth(healthDiff); // Restore or not, health when increased?
                tradingPoints--;
            }

            if (GUILayout.Button($"Regen + {regenPercent}%")) // Uncommon
            {
                playerAttributes.HealthAttributes.HealthRegenerationPercent *= 1 + regenPercent;
                regenPercent = Random.Range(2, 5) / 100f;
                tradingPoints--;
            }

            if (GUILayout.Button($"Regeneration Rate - {healthRegenerationCooldownPercent}%")) // Uncommon
            {
                playerAttributes.HealthAttributes.HealthRegenerationCooldown *= 1 - healthRegenerationCooldownPercent;
                healthRegenerationCooldownPercent = Random.Range(2, 5) / 100f;
                tradingPoints--;
            }

            //public float FearIncrease { get; set; }
            //public float Calmness { get; set; }
            //public float CalmnessRegenerationAmount { get; set; }
            //public float CalmnessRegenerationCooldown { get; set; }

            GUILayout.EndArea();

            #endregion

            #region Movement

            GUILayout.BeginArea(new Rect(400, 0, 200, 200));

            GUILayout.TextArea("MOVEMENT");

            //if (GUILayout.Button($"Stamina + {staminaPercent}%")) // Rare
            //{
            //    var StaminaBeforeUpdate = playerAttributes.MovementAttributes.Stamina;
            //    playerAttributes.MovementAttributes.Stamina *= 1 + staminaPercent;
            //    var StaminaDiff = Mathf.RoundToInt(playerAttributes.MovementAttributes.Stamina - StaminaBeforeUpdate);
            //    healthPercent = Random.Range(2, 5) / 100f;
            //    healthSystem.UpdateHealth(StaminaDiff); // Change to movementSystem
            //    tradingPoints--;
            //}


            //if (GUILayout.Button($"Stamina Regen + {staminaRegenPercent}%")) // Uncommon
            //{
            //    playerAttributes.MovementAttributes.StaminaRegenerationAmount *= 1 + staminaRegenPercent;
            //    staminaRegenPercent = Random.Range(2, 5) / 100f;
            //    tradingPoints--;
            //}

            //public float StaminaRegenerationCooldown { get; set; }

            if (GUILayout.Button($"Speed + {moveSpeedPercent}%")) // Unreal
            {
                playerAttributes.MovementAttributes.MoveSpeed *= 1 + moveSpeedPercent;
                moveSpeedPercent = Random.Range(1, 4) / 100f;
                tradingPoints--;
            }

            //public float BoostSpeedMultiplier { get; set; }
            //public float BoostStaminaCost { get; set; }
            //public float DashSpeedMultiplier { get; set; }
            //public float DashStaminaCost { get; set; }
            //public float DashCooldown { get; set; }
            //public float DashDuration { get; set; }

            GUILayout.EndArea();

            #endregion

        }
    }
}
