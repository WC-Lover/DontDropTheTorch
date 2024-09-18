using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TradingSystem : NetworkBehaviour
{

    TradingSystem Instance;
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
    public int tradingPoints;

    private List<Transform> otherPlayers;

    private float dmgPercent;
    private float healthPercent;
    private float regenPercent;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        this.playerAttributes = playerAttributes;

        traderSpawnTime = this.playerAttributes.TradingAttributes.TraderSpawnTime;
        tradingTime = this.playerAttributes.TradingAttributes.TradingTime;

        isTrading = false;
        isInsideTradingZone = false;
        isTraderSpawned = false;

        playersInsideTradingZone = 0;
        tradingPoints = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Instance = this;
        otherPlayers = EnemySpawnSystem.Instance.otherPlayers;
        healthSystem = GetComponent<HealthSystem>();
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

            if (IsServer && !isTrading && playersInsideTradingZone == otherPlayers.Count + 1) // +1 for local player
            {
                ReviveDeadPlayers();
                StartTrading();
                StartTradingRpc();
            }

            if (isTrading)
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

        Instance.dmgPercent = Random.Range(10, 20) / 100f;
        Instance.healthPercent = Random.Range(2, 5) / 100f;
        Instance.regenPercent = Random.Range(2, 5) / 100f;
    }

    private void ReviveDeadPlayers()
    {
        foreach (Transform player in otherPlayers)
        {
            var deadPlayer = player.GetComponent<HealthSystem>();
            if (deadPlayer.IsDead)
            {
                deadPlayer.RevivePlayer();
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void PlayerHasEnteredTradingZoneRpc()
    {
        playersInsideTradingZone++;
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
            trader.transform.position = new Vector2(mostRightPlayerX + mostRightPlayerBeamDistance, 0); // change +2 to the length of the light beam
            traderNetworkObject = trader.GetComponent<NetworkObject>();
            traderNetworkObject.Spawn();
        }
        
        isTraderSpawned = true;
        // 0.5f is wall width, player needs to be inside zone, not in wall
        // lengthOfTheTradingArea/2 = 5
        tradingZoneAreaStartX = mostRightPlayerX + mostRightPlayerBeamDistance + 0.5f - 5;
    }

    void OnGUI()
    {
        if (isTrading) //  && tradingPoints > 0
        {
            GUILayout.BeginArea(new Rect(310, 10, 300, 300));

            #region Weapon

            if (GUILayout.Button($"DMG + {dmgPercent}%"))
            {
                playerAttributes.WeaponAttributes.Damage *= 1 + dmgPercent;
                dmgPercent = Random.Range(10, 20) / 100f;
                tradingPoints--;
            }

            //if (GUILayout.Button("Penetration"))
            //{
            //    Instance.Penetration += 1;
            //    GameController.Instance.Score--;
            //}

            #endregion

            #region Health

            if (GUILayout.Button($"HP + {healthPercent}%"))
            {
                var healthBeforeUpdate = playerAttributes.HealthAttributes.HealthAmount;
                playerAttributes.HealthAttributes.HealthAmount *= 1 + healthPercent;
                var healthDiff = Mathf.RoundToInt(playerAttributes.HealthAttributes.HealthAmount - healthBeforeUpdate);
                healthPercent = Random.Range(2, 5) / 100f;
                healthSystem.UpdateHealth(healthDiff); // Restore or not, health when increased?
                tradingPoints--;
            }

            if (GUILayout.Button($"HP Regen + {regenPercent}%"))
            {
                playerAttributes.HealthAttributes.HealthRegenerationPercent *= 1 + regenPercent;
                regenPercent = Random.Range(2, 5) / 100f;
                tradingPoints--;
            }

            #endregion

            GUILayout.EndArea();
        }
    }
}
