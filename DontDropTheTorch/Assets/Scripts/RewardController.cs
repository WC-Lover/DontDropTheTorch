using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RewardController : NetworkBehaviour
{
    private int pointsAmount;
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

            ts.ChangeTradingPoints(pointsAmount);

            NetworkObject.Despawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        rewardSFX = GetComponent<RewardSFXController>();
        rewardSFX.CoinDropSFX();
        if (IsServer) pointsAmount = TradingSystem.Instance.VaweCounter;
        PickUpTimer = 10f;
    }

    public void Update()
    {
        if (!IsServer) return;

        if (PickUpTimer > 0) PickUpTimer -= Time.deltaTime;
        else
        {
            TradingSystem.Instance.NotPickedUpCoins++;
            NetworkObject.Despawn();
        }
    }
}
