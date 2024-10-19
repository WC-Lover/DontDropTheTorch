using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObstacleSpawner : NetworkBehaviour
{
    public static ObstacleSpawner Instance;

    [SerializeField] private GameObject[] obstacles;
    private List<Transform> otherPlayers;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int obstacleCount = 6;
    [SerializeField] private float minDistanceBetweenObstacles = 3.5f;
    [SerializeField] private float minDistanceFromPlayers = 5f;
    [SerializeField] private List<Vector2> placedObstacles = new List<Vector2>();

    [SerializeField] float spawnTimer = 0;
    [SerializeField] float spawnTimerMax = 4f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Instance = this;
        otherPlayers = LobbyManager.LobbyPlayersTransforms;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimer > 0) spawnTimer -= Time.deltaTime;
        else
        {
            spawnTimer = spawnTimerMax;
            SpawnObstaclesAroundPlayer();
        }
    }

    void SpawnObstaclesAroundPlayer()
    {
        int attempts = 0;
        int createdObstacles = 0;

        while (createdObstacles < obstacleCount && attempts < obstacleCount * 10)
        {
            Vector2 randomPosition = Random.insideUnitCircle.normalized * Random.Range(0f, spawnRadius);
            Vector2 spawnPosition = (Vector2)transform.position + randomPosition;

            if (IsValidPosition(spawnPosition))
            {
                GameObject obstaclePrefab = obstacles[Random.Range(0, obstacles.Length)];

                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);

                placedObstacles.Add(spawnPosition);

                createdObstacles++;
            }

            attempts++;
        }
    }

    bool IsValidPosition(Vector2 position)
    {
        foreach (Transform otherPlayer in otherPlayers)
        {
            if (Vector2.Distance(position, otherPlayer.position) < minDistanceFromPlayers)
            {
                return false;
            }
        }

        foreach (Vector2 placedPosition in placedObstacles)
        {
            if (Vector2.Distance(position, placedPosition) < minDistanceBetweenObstacles)
            {
                return false;
            }
        }

        return true; 
    }
}
