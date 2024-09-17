using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyController : NetworkBehaviour
{
    private NetworkObject networkObject;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool DealDamageToEnemyRpc(float damage)
    {
        throw new NotImplementedException();
    }

    public void DespawnEnemy()
    {
        networkObject.Despawn();
    }
}
