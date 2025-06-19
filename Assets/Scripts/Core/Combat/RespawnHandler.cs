using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private PlayerTank playerPrefab;
    [SerializeField] private float keptCoinsPercentage = 0.25f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PlayerTank[] players = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None);

        foreach (PlayerTank player in players)
        {
            HandlePlayerSpawn(player);
        }

        PlayerTank.OnPlayerSpawned += HandlePlayerSpawn;
        PlayerTank.OnPlayerDespawned += HandlePlayerDespawn;

    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        PlayerTank.OnPlayerSpawned -= HandlePlayerSpawn;
        PlayerTank.OnPlayerDespawned -= HandlePlayerDespawn;
    }

    private void HandlePlayerSpawn(PlayerTank player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawn(PlayerTank player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(PlayerTank player)
    {
        int keepCoins = player.Wallet.TotalCoins.Value * (int)keptCoinsPercentage;

        Destroy(player.gameObject);

        StartCoroutine(SpawnPlayer(player.OwnerClientId, keepCoins));
    }

    private IEnumerator SpawnPlayer(ulong ownerId, int keepCoins)
    {
        yield return null;

        PlayerTank player = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPointPos(), Quaternion.identity);

        player.NetworkObject.SpawnAsPlayerObject(ownerId);

        player.Wallet.TotalCoins.Value = keepCoins;
    }


}
