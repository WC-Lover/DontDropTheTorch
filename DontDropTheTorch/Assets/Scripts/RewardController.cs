using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RewardController : NetworkBehaviour
{
    private RewardSFXController rewardSFX;
    private float PickUpTimer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<TradingSystem>(out var ts))
        {
            if (ts.IsOwner)
            {
                rewardSFX.CoinCollectSFX();
                gameObject.SetActive(false);
            }

            if (!IsServer) return;

            ts.ChangeTradingPoints(1);

            NetworkObject.Despawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        rewardSFX = GetComponent<RewardSFXController>();
        rewardSFX.CoinDropSFX();

        PickUpTimer = 1000f;
    }

    public void Update()
    {
        if (!IsServer) return;

        if (PickUpTimer > 0) PickUpTimer -= Time.deltaTime;
        else NetworkObject.Despawn();
    }
}
