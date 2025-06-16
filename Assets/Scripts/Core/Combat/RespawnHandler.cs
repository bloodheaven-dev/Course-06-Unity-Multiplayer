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

        PlayerLogic[] players = FindObjectsByType<PlayerLogic>(FindObjectsSortMode.None);

        foreach (PlayerLogic player in players)
            HandlePlayerSpawn(player);
        }

        PlayerLogic.OnPlayerSpawned += HandlePlayerSpawn;
        PlayerLogic.OnPlayerDespawned += HandlePlayerDespawn;

    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        PlayerLogic.OnPlayerSpawned -= HandlePlayerSpawn;
        PlayerLogic.OnPlayerDespawned -= HandlePlayerDespawn;
    }

    private void HandlePlayerSpawn(PlayerLogic player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawn(PlayerLogic player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(PlayerLogic player)
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
