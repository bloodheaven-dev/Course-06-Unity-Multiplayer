using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] RespawningCoin coinPrefab;

    [SerializeField] int maxCoins = 50;
    [SerializeField] int coinValue = 10;

    [SerializeField] Vector2 xSpawnRange;
    [SerializeField] Vector2 ySpawnRange;

    [SerializeField] LayerMask layerMask;

    float coinRadius;

    Collider2D[] colliders = new Collider2D[1];


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }


    void SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
        

        coinInstance.SetValue(coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn();

        coinInstance.transform.parent = this.transform;

        coinInstance.OnCollected += HandleCoinCollected;
    }


    Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        while (true)
        {
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            Vector2 spawnPoint = new Vector2(x, y);
            
            colliders = Physics2D.OverlapCircleAll(spawnPoint, coinRadius, layerMask);

            if(colliders.Length == 0)
            {
                return spawnPoint;
            }

        }

    }
    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }




}
