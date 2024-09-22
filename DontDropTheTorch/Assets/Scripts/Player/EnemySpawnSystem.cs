using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;
using System;

public class EnemySpawnSystem : NetworkBehaviour
{
    public static EnemySpawnSystem Instance;

    private EnemySpawnAttributes enemySpawnAttributes;
    // Variables to spawn emeies
    // What?
    [SerializeField] private Transform prefabToSpawn;
    // When?
    [SerializeField] private float spawnCooldown;
    [SerializeField] private float waveSpawnCooldown;

    public List<Transform> otherPlayers;

    // Local player properties
    private float playerLightBeamDistance;
    private float playerLightBeamAngle;
    private float playerLightDirectionAngle;

    public List<EnemyController> enemyControllers;
    public bool despawnAllEnemies;
    public bool disableEnemySpawn;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        enemySpawnAttributes = playerAttributes.EnemySpawnAttributes;

        spawnCooldown = enemySpawnAttributes.SpawnCooldown;
        waveSpawnCooldown = enemySpawnAttributes.WaveSpawnCooldown;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Instance = this;

        otherPlayers = LobbyManager.LobbyPlayersTransforms;

        enemyControllers = new List<EnemyController>();
        // currently 4 -> distance from player to corner where spawn is sqrt(4^2 + 4^2) = 5.65(roughly)
        var playerLight = GetComponentInChildren<Light2D>();
        playerLightBeamDistance = playerLight.pointLightOuterRadius;
        playerLightBeamAngle = playerLight.pointLightOuterAngle;
        playerLightDirectionAngle = transform.eulerAngles.z;
    }

    private void Update()
    {
        if (!IsOwner) return;

        #region Despawn Enemies if Trading has Started

        if (IsServer && despawnAllEnemies && enemyControllers.Count > 0)
        {
            foreach (EnemyController ec in enemyControllers.ToArray()) ec.DespawnEnemy();
            enemyControllers.Clear();
        }

        #endregion

        if (disableEnemySpawn) return;

        if (spawnCooldown > 0) spawnCooldown -= Time.deltaTime;
        else SpawnNearbyZombies();
        
    }

    private void SpawnNearbyZombies()
    {
        // Re-set spawn cooldown
        spawnCooldown = enemySpawnAttributes.SpawnCooldown;
        // Player's light direction angle
        playerLightDirectionAngle = transform.eulerAngles.z;
        // Player's position
        var localPlayerPosition = transform.position;

        List<Vector3> freeZoneList = GetPlayerFreeSpawnZones();
        if (freeZoneList.Count < 1) return;

        List<Transform> nearbyPlayers = new List<Transform>();
        List<Transform> farPlayers = new List<Transform>(); // Spawn in between players

        #region Find Nearby Players and Remove according Zones

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            Vector3 playerPosition = otherPlayers[i].position;
            // Define direction and distance from LocalPlayer to NearbyPlayer
            Vector2 directionFromLocalPlayerToOther = playerPosition - localPlayerPosition;

            float distanceFromLocalPlayerToOtherX = directionFromLocalPlayerToOther.x;
            float distanceFromLocalPlayerToOtherY = directionFromLocalPlayerToOther.y;

            // Define which Corner is probably Not Available for Enemy Spawn
            float directionNearbyToLocalPlayerX = directionFromLocalPlayerToOther.x > 0 ? 4 : -4;
            float directionNearbyToLocalPlayerY = directionFromLocalPlayerToOther.y > 0 ? 4 : -4;

            if (Math.Abs(distanceFromLocalPlayerToOtherX) + 0.5f <= playerLightBeamDistance &&
                Math.Abs(distanceFromLocalPlayerToOtherY) + 0.5f <= playerLightBeamDistance)
            {
                // Can't spawn in between players, remove zone from list
                freeZoneList.Remove(new Vector3(directionNearbyToLocalPlayerX, directionNearbyToLocalPlayerY));
            }
            else if (Math.Abs(distanceFromLocalPlayerToOtherX) <= playerLightBeamDistance * 2 &&
                Math.Abs(distanceFromLocalPlayerToOtherY) <= playerLightBeamDistance * 2)
            {
                // Define which Direction from Nearby Player side Covers Local Player Enemy Spawn corner
                float directionX = 1; // 1 - right | -1 - left
                float directionY = 1; // 1 - top | -1 - bottom

                // Left Corner for Nearby Player
                if (distanceFromLocalPlayerToOtherX > 4) directionX = -1;
                // Right Corner for Nearby Player
                else if (distanceFromLocalPlayerToOtherX > 0) directionX = 1;
                // Left Corner for Nearby Player
                else if (distanceFromLocalPlayerToOtherX > -4) directionX = -1;
                // Right Corner for Nearby Player
                else directionX = 1;

                // Bottom Corner for Nearby Player
                if (distanceFromLocalPlayerToOtherY > 4) directionY = -1;
                // Upper Corner for Nearby Player
                else if (distanceFromLocalPlayerToOtherY > 0) directionY = 1;
                // Bottom Corner for Nearby Player
                else if (distanceFromLocalPlayerToOtherY > -4) directionY = -1;
                // Upper Corner for Nearby Player
                else directionY = 1;

                // Define which Direction is Focused by Nearby Player
                float nearPlayerEulerAngleZ = otherPlayers[i].eulerAngles.z;

                // Right-Top corner
                if (nearPlayerEulerAngleZ >= 0 && nearPlayerEulerAngleZ < 90
                    && directionX == 1 && directionY == 1)
                    freeZoneList.Remove(new Vector3(directionNearbyToLocalPlayerX, directionNearbyToLocalPlayerY));
                // Left-Top corner
                else if (nearPlayerEulerAngleZ >= 90 && nearPlayerEulerAngleZ < 180
                    && directionX == -1 && directionY == 1)
                    freeZoneList.Remove(new Vector3(directionNearbyToLocalPlayerX, directionNearbyToLocalPlayerY));
                // Left-Bottom corner
                else if (nearPlayerEulerAngleZ >= 180 && nearPlayerEulerAngleZ < 270
                    && directionX == -1 && directionY == -1)
                    freeZoneList.Remove(new Vector3(directionNearbyToLocalPlayerX, directionNearbyToLocalPlayerY));
                // Right-Bottom corner
                else if (directionX == 1 && directionY == -1)
                    freeZoneList.Remove(new Vector3(directionNearbyToLocalPlayerX, directionNearbyToLocalPlayerY));
            }
        }

        #endregion

        #region Spawn Enemies if Player goes too high or low

        // Spawn if player is approaching the limit? like prediction? then check on Host/Server side?

        //if (localPlayerTransform != null) 
        //{

        //    var localPlayerX = localPlayerTransform.position.x;
        //    var localPlayerY = localPlayerTransform.position.y;

        //    var enemyToSpawnX = localPlayerX;
        //    var enemyToSpawnY = 0f;
        //    if (localPlayerTransform.position.y > 3)
        //    {
        //        enemyToSpawnY = localPlayerY + 3;
        //    }
        //    else if (localPlayerTransform.position.y < -3)
        //    {
        //        enemyToSpawnY = localPlayerY - 3;
        //    }
        //    else
        //    {
        //        return;
        //    }
        //    for (int i = -1; i < 2; i++)
        //    {
        //        Vector2 spawnPos = new Vector2(enemyToSpawnX + i, enemyToSpawnY);
        //        SpawnEnemyServerRpc(spawnPos);
        //    }
        //    _spawnCooldown = spawnFrequencyInSeconds;
        //}

        #endregion

        #region Spawn Enemies in free zones

        //var tradingZoneStartX = TradingController.Instance.TradingZoneStartX;
        for (int i = 0; i < freeZoneList.Count/4; i++) // TODO: Remove /4, done to spawn only 1 zombie per player
        {
            Vector2 freeZone = freeZoneList[i];
            Vector3 freeZoneVector3 = new Vector3(freeZone.x, freeZone.y, 0);
            int distance = (int)Math.Abs(freeZone.x); // length = width
            for (int j = 0; j < distance / 4; j++) // distance / 4 for now so only 1 enemie is spawned in each corner
            {
                Vector2 enemySpawnVector = localPlayerPosition + freeZoneVector3;

                // Spawn enemy in available area, starting from farest point, not to spawn infront of the player if it is moving!
                //if (tradingZoneStartX > enemySpawnVector.x)
                if (IsServer) SpawnEnemy(enemySpawnVector);
                else SpawnEnemyRpc(enemySpawnVector);
            }
        }

        #endregion
    }

    [Rpc(SendTo.Server)]
    private void SpawnEnemyRpc(Vector2 enemySpawnPos)
    {
        SpawnEnemy(enemySpawnPos);
    }

    private void SpawnEnemy(Vector2 spawnPos)
    {
        Transform objectToSpawn = Instantiate(prefabToSpawn);
        objectToSpawn.position = spawnPos;
        Instance.enemyControllers.Add(objectToSpawn.GetComponent<EnemyController>());
        objectToSpawn.GetComponent<NetworkObject>().Spawn();
    }

    private List<Vector3> GetPlayerFreeSpawnZones()
    {
        List<Vector3> playerFreeZones;

        float playerLightBeamAngleFrom = playerLightDirectionAngle - playerLightBeamAngle / 2;
        float playerLightBeamAngleTo = playerLightDirectionAngle + playerLightBeamAngle / 2;

        float zoneSize = playerLightBeamDistance; // current: 4

        Vector3 topLeft = new Vector3(-zoneSize, zoneSize, 0); // Vector(-length, height);
        Vector3 topRight = new Vector3(zoneSize, zoneSize, 0); //Vector(length, height);
        Vector3 bottomRight = new Vector3(zoneSize, -zoneSize, 0); //Vector(length, -height); 
        Vector3 bottomleft = new Vector3(-zoneSize, -zoneSize, 0); //Vector(-length, -height);

        /*
         * Player actually never sees an enemy being spawned!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         * In corners light can reach ONLY IF player has light beam distance update to some degree, otherwise light is too short.
        */
        // Exclude zone which is focuesed by player's light beam
        //// Looking Top Right
        //if (playerLightBeamAngleFrom >= 0 && playerLightBeamAngleTo <= 90)
        //    playerFreeZones = new List<Vector3>() { topLeft, bottomRight, bottomleft };
        //// Looking Top Left
        //else if (playerLightBeamAngleFrom >= 90 && playerLightBeamAngleTo <= 180)
        //    playerFreeZones = new List<Vector3>() { topRight, bottomRight, bottomleft };
        //// Looking Bottom Left
        //else if (playerLightBeamAngleFrom >= 180 && playerLightBeamAngleTo <= 270)
        //    playerFreeZones = new List<Vector3>() { topLeft, topRight, bottomRight };
        //// Looking Bottom Right
        //else if (playerLightBeamAngleFrom >= 270 && playerLightBeamAngleTo <= 360)
        //    playerFreeZones = new List<Vector3>() { topLeft, topRight, bottomleft };
        //else
        //    playerFreeZones = new List<Vector3>() { topLeft, topRight, bottomleft, bottomRight };

        playerFreeZones = new List<Vector3>() { topLeft, topRight, bottomleft, bottomRight };
        return playerFreeZones;
    }

}
