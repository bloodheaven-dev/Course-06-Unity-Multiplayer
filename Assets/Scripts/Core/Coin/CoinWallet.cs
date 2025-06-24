using JetBrains.Annotations;
using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    [Header("References")]

    [SerializeField] Health health;
    [SerializeField] BountyCoin coinPrefab;

    [Header("Settings")]

    [SerializeField] private float bountyPercentage = 0.25f;
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] LayerMask layerMask;

    float coinRadius;

    Collider2D[] colliders = new Collider2D[1];

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        health.OnDie -= HandleDie;
    }

    private void HandleDie(Health health)
    {
        
        int bountyValue = (int)(TotalCoins.Value * bountyPercentage);
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) return;

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out Coin coin)) return;

        int coinValue = coin.Collect();

        if (!IsServer) return;

        TotalCoins.Value += coinValue;


    }

    public void SpendCoins(int value)
    {
        TotalCoins.Value -= value;
    }

    Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            colliders = Physics2D.OverlapCircleAll(spawnPoint, coinRadius, layerMask);

            if (colliders.Length == 0)
            {
                return spawnPoint;
            }

        }

    }
}
