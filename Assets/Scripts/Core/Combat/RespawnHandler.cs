using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

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
        Destroy(player.gameObject);

        StartCoroutine(SpawnPlayer(player.OwnerClientId));
    }

    private IEnumerator SpawnPlayer(ulong ownerId)
    {
        yield return null;

        NetworkObject player = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPointPos(), Quaternion.identity);

        player.SpawnAsPlayerObject(ownerId);
    }


}
