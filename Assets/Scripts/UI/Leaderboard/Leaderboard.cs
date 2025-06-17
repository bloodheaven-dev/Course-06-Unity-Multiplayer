using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] Transform leaderboardItemContent;
    [SerializeField] LeaderboardItemDisplay leaderboardItemPrefab;

    private NetworkList<LeaderboardItemState> leaderboardItems;
    private List<LeaderboardItemDisplay> itemDisplays = new List<LeaderboardItemDisplay>();

    private void Awake()
    {
        leaderboardItems = new NetworkList<LeaderboardItemState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardItems.OnListChanged += HandleLeaderboardItemsChanged;

            foreach(LeaderboardItemState item in leaderboardItems)
            {
                HandleLeaderboardItemsChanged(new NetworkListEvent<LeaderboardItemState>
                {
                    Type = NetworkListEvent<LeaderboardItemState>.EventType.Add,
                    Value = item
                });
            }
        }

        if (!IsServer) return;

        PlayerTank[] players = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None);

        foreach (PlayerTank player in players)
        {
            HandlePlayerSpawned(player);
        }


        PlayerTank.OnPlayerSpawned += HandlePlayerSpawned;
        PlayerTank.OnPlayerDespawned += HandlePlayerDespawned;
    }



    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardItems.OnListChanged -= HandleLeaderboardItemsChanged;
        }

        if (!IsServer) return;

        PlayerTank.OnPlayerSpawned -= HandlePlayerSpawned;
        PlayerTank.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandleLeaderboardItemsChanged(NetworkListEvent<LeaderboardItemState> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case (NetworkListEvent<LeaderboardItemState>.EventType.Add):

                if (!itemDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {

                    LeaderboardItemDisplay leaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardItemContent);

                    leaderboardItem.Init(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);

                    itemDisplays.Add(leaderboardItem);

                }

                break;

            case (NetworkListEvent<LeaderboardItemState>.EventType.Remove):

                LeaderboardItemDisplay displayToRemove = itemDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToRemove == null) break;

                displayToRemove.transform.SetParent(null);
                itemDisplays.Remove(displayToRemove);
                Destroy(displayToRemove.gameObject);

                break;

            case (NetworkListEvent<LeaderboardItemState>.EventType.Value):

                LeaderboardItemDisplay displayToUpdate = itemDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToUpdate == null) break;

                displayToUpdate.UpdateCoins(changeEvent.Value.Coins);

                break;
        }
    }

    private void HandlePlayerSpawned(PlayerTank player)
    {
        leaderboardItems.Add(new LeaderboardItemState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
    }

    private void HandlePlayerDespawned(PlayerTank player)
    {
        if (leaderboardItems == null) return;

        foreach(LeaderboardItemState entity in leaderboardItems)
        {
            if (entity.ClientId != player.OwnerClientId) continue;

            leaderboardItems.Remove(entity);

            break;
        }
    }
}
