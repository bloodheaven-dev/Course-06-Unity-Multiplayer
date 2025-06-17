using System;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] Transform leaderboardItemContent;
    [SerializeField] LeaderboardItem leaderboardItemPrefab;

    private NetworkList<LeaderboardItemState> leaderboardItems;

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

        PlayerLogic[] players = FindObjectsByType<PlayerLogic>(FindObjectsSortMode.None);

        foreach (PlayerLogic player in players)
        {
            HandlePlayerSpawned(player);
        }


        PlayerLogic.OnPlayerSpawned += HandlePlayerSpawned;
        PlayerLogic.OnPlayerDespawned += HandlePlayerDespawned;
    }



    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardItems.OnListChanged -= HandleLeaderboardItemsChanged;
        }

        if (!IsServer) return;

        PlayerLogic.OnPlayerSpawned -= HandlePlayerSpawned;
        PlayerLogic.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandleLeaderboardItemsChanged(NetworkListEvent<LeaderboardItemState> changeEvent)
    {
        switch(changeEvent.Type)
        {
            case (NetworkListEvent<LeaderboardItemState>.EventType.Add):

                Instantiate(leaderboardItemPrefab, leaderboardItemContent);

                break;
        }
    }

    private void HandlePlayerSpawned(PlayerLogic player)
    {
        leaderboardItems.Add(new LeaderboardItemState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
    }

    private void HandlePlayerDespawned(PlayerLogic player)
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
