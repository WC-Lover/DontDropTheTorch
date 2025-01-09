using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using TMPro;


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
    public int NotPickedUpCoins;
    public int UnusedTradingPoints;
    public int VaweCounter;

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
    public NetworkVariable<int> tradingPoints = new NetworkVariable<int>(); // make NetworkVariable and update on server detects kill!

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

    private Dictionary<string, float> updatedAttributes;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        this.playerAttributes = playerAttributes;

        traderSpawnTime = playerAttributes.TradingAttributes.TraderSpawnTime;
        tradingTime = playerAttributes.TradingAttributes.TradingTime;

        isTrading = false;
        isInsideTradingZone = false;
        isTraderSpawned = false;

        playersInsideTradingZone = 0;
        if (IsServer) tradingPoints.Value = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (IsOwner && !IsServer) AddPlayerAliveRpc();

        Instance = this;
        otherPlayers = LobbyManager.LobbyPlayersTransforms;
        healthSystem = GetComponent<HealthSystem>();
        playersAlive = 1;
        NotPickedUpCoins = 0;
        UnusedTradingPoints = 0;
        VaweCounter = 0;
        tradingPoints.OnValueChanged += OnTradingPointsChangeValue;
        updatedAttributes = new();
    }

    private void OnTradingPointsChangeValue(int previousValue, int newValue)
    {
        if (!IsServer) return;

        tradingPoints.Value = newValue;
    }

    public void ChangeTradingPoints(int pointsAmount)
    {
        if (!IsServer) ChangeTradingPointsRpc(pointsAmount);
        else tradingPoints.Value += pointsAmount;
    }

    [Rpc(SendTo.Server)]
    private void ChangeTradingPointsRpc(int pointsAmount)
    {
        tradingPoints.Value += pointsAmount;
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
                SplitNotPickedUpCoins();
                UpdateEnemies();
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

    private void SplitNotPickedUpCoins()
    {
        if (NotPickedUpCoins == 0) return;

        int amountPerPlayer = NotPickedUpCoins / playersAlive;
        tradingPoints.Value += amountPerPlayer;
        foreach (Transform trans in otherPlayers)
        {
            trans.GetComponent<TradingSystem>().tradingPoints.Value += amountPerPlayer;
        }

        NotPickedUpCoins = 0;
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
        if (IsServer)
        {
            UnusedTradingPoints = 0;
            VaweCounter++;

            otherPlayers.Add(transform);
            foreach (Transform player in otherPlayers)
            {
                UnusedTradingPoints += player.GetComponent<TradingSystem>().tradingPoints.Value;
            }
            otherPlayers.Remove(transform);

            traderNetworkObject.Despawn();

        }
    }

        private void UpdateEnemies()
    {
        int points = NotPickedUpCoins - UnusedTradingPoints;

        otherPlayers.Add(transform);
        foreach (Transform player in otherPlayers)
        {
            points += player.GetComponent<TradingSystem>().tradingPoints.Value;
        }
        otherPlayers.Remove(transform);

        int attributesAmount = 6;
        int multiplier = points / attributesAmount;

        if (multiplier < 1) multiplier = 1;

        // formula for percentage of update per vawe
        float percentage = 1 + (0.05f * multiplier * VaweCounter * 3 / 4);
        //Debug.Log($"percentage -> {percentage}");
        EnemySpawnSystem.Instance.enemyAttributes.Speed *= percentage;
        EnemySpawnSystem.Instance.enemyAttributes.Damage *= percentage;
        EnemySpawnSystem.Instance.enemyAttributes.Health *= percentage;
        //EnemySpawnSystem.Instance.enemyAttributes.AttackRange *= percentage;
        EnemySpawnSystem.Instance.enemyAttributes.LeapRange *= percentage;
        EnemySpawnSystem.Instance.enemyAttributes.Defence *= percentage;
        EnemySpawnSystem.Instance.enemyAttributes.AttackCooldown *= 2 - percentage;
        EnemySpawnSystem.Instance.enemyAttributes.AttackPushMultiplier *= percentage;
        //Debug.Log($"Speed -> {EnemySpawnSystem.Instance.enemyAttributes.Speed}");
        //Debug.Log($"Damage -> {EnemySpawnSystem.Instance.enemyAttributes.Damage}");
        //Debug.Log($"Health -> {EnemySpawnSystem.Instance.enemyAttributes.Health}");
        //Debug.Log($"AttackRange -> {EnemySpawnSystem.Instance.enemyAttributes.AttackRange}");
        //Debug.Log($"LeapRange -> {EnemySpawnSystem.Instance.enemyAttributes.LeapRange}");
        //Debug.Log($"Defence -> {EnemySpawnSystem.Instance.enemyAttributes.Defence}");
        //Debug.Log($"AttackCooldown -> {EnemySpawnSystem.Instance.enemyAttributes.AttackCooldown}");
        //Debug.Log($"AttackPushMultiplier -> {EnemySpawnSystem.Instance.enemyAttributes.AttackPushMultiplier}");

        EnemySpawnSystem.Instance.enemyPerPlayer++;
        //Debug.Log($"enemyPerPlayer -> {EnemySpawnSystem.Instance.enemyPerPlayer}");
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

        Instance.dmgPercent = Random.Range(5, 10) / 100f;
        Instance.projectileAngleSpreadPercent = Random.Range(2, 7) / 100f;
        Instance.rangePercent = Random.Range(5, 10) / 100f;
        Instance.critPercent = Random.Range(5, 10) / 100f;
        Instance.critChancePercent = Random.Range(5, 10) / 100f;
        Instance.fireRatePercent = Random.Range(5, 10) / 100f;
        Instance.reloadTimePercent = Random.Range(5, 10) / 100f;

        #endregion

        #region Health

        Instance.healthPercent = Random.Range(2, 5) / 100f;
        Instance.regenPercent = Random.Range(2, 5) / 100f;

        #endregion

        #region Movement

        Instance.moveSpeedPercent = Random.Range(2, 6) / 100f;

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
            trader.transform.position = new Vector2(mostRightPlayerX + mostRightPlayerBeamDistance + 10, 0); // change +2 to the length of the light beam
            traderNetworkObject = trader.GetComponent<NetworkObject>();
            traderNetworkObject.Spawn();
        }

        Instance.isTraderSpawned = true;
        // 0.5f is wall width, player needs to be inside zone, not in wall
        // lengthOfTheTradingArea/2 = 5
        Instance.tradingZoneAreaStartX = mostRightPlayerX + mostRightPlayerBeamDistance + 0.5f - 5 + 10;
    }

    [Rpc(SendTo.Server)]
    private void AddPlayerAliveRpc()
    {
        Instance.playersAlive++;
    }

    void OnGUI()
    {
        if (IsOwner && isTrading && tradingPoints.Value > 0) //  && tradingPoints > 0
        {

            #region Weapon

            GUILayout.BeginArea(new Rect(0, 0, 200, 200));

            GUILayout.TextArea("WEAPON");

            if (GUILayout.Button($"Damage + {dmgPercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.Damage *= 1 + dmgPercent;
                dmgPercent = Random.Range(5, 10) / 100f;
                updatedAttributes["Damage"] = playerAttributes.WeaponAttributes.Damage;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Projectile Amount + 1")) // Rare // Can't update if not enough bulets?
            {
                playerAttributes.WeaponAttributes.ProjectileAmount++;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Clip capacity + 1")) // Uncommon
            {
                playerAttributes.WeaponAttributes.ClipCapacity++;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Projectile Angle + {projectileAngleSpreadPercent}%")) // Common
            {
                playerAttributes.WeaponAttributes.ProjectileSpreadAngle *= 1 + projectileAngleSpreadPercent;
                projectileAngleSpreadPercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Penetration + 1")) // Rare
            {
                playerAttributes.WeaponAttributes.Penetration++;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Range + {rangePercent}%")) // Common
            {
                playerAttributes.WeaponAttributes.Range *= 1 + rangePercent;
                rangePercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Crit + {critPercent}%")) // Rare
            {
                playerAttributes.WeaponAttributes.Crit *= 1 + critPercent;
                critPercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Crit chance + {critChancePercent}%")) // Rare
            {
                playerAttributes.WeaponAttributes.CritChance *= 1 + critChancePercent;
                critChancePercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Fire rate + {fireRatePercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.FireRate *= 1 - fireRatePercent;
                fireRatePercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Reload time - {reloadTimePercent}%")) // Uncommon
            {
                playerAttributes.WeaponAttributes.ReloadTime *= 1 - reloadTimePercent;
                reloadTimePercent = Random.Range(5, 10) / 100f;
                ChangeTradingPoints(-1);
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
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Regen + {regenPercent}%")) // Uncommon
            {
                playerAttributes.HealthAttributes.HealthRegenerationPercent *= 1 + regenPercent;
                regenPercent = Random.Range(2, 5) / 100f;
                ChangeTradingPoints(-1);
            }

            if (GUILayout.Button($"Regeneration Rate - {healthRegenerationCooldownPercent}%")) // Uncommon
            {
                playerAttributes.HealthAttributes.HealthRegenerationCooldown *= 1 - healthRegenerationCooldownPercent;
                healthRegenerationCooldownPercent = Random.Range(2, 5) / 100f;
                ChangeTradingPoints(-1);
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
                moveSpeedPercent = Random.Range(2, 6) / 100f;
                ChangeTradingPoints(-1);
            }

            //public float BoostSpeedMultiplier { get; set; }
            //public float BoostStaminaCost { get; set; }
            //public float DashSpeedMultiplier { get; set; }
            //public float DashStaminaCost { get; set; }
            //public float DashCooldown { get; set; }
            //public float DashDuration { get; set; }

            GUILayout.EndArea();

            #endregion

            GUILayout.BeginArea(new Rect(700, 0, 100, 50));

            GUILayout.TextArea($"Points: {tradingPoints.Value}");

            GUILayout.EndArea();

        }

        if (!isTrading && IsOwner)
        {

            GUILayout.BeginArea(new Rect(100, 0, 100, 50));

            GUILayout.TextArea($"Points: {tradingPoints.Value}");

            GUILayout.EndArea();

        }
    }
}
