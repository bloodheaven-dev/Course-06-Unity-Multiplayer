using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerLeaderboardItemContent;
    [SerializeField] Transform teamLeaderboardItemContent;
    [SerializeField] GameObject teamLeaderboard;
    [SerializeField] LeaderboardItemDisplay leaderboardItemPrefab;
    [SerializeField] TeamColorLookup teamColorLookup;

    [Header("Settings")]
    [SerializeField] Color ownerColor;
    [SerializeField] string[] teamNames;

    private NetworkList<LeaderboardItemState> leaderboardItems;
    private List<LeaderboardItemDisplay> playerItemDisplays = new List<LeaderboardItemDisplay>();
    private List<LeaderboardItemDisplay> teamItemDisplays = new List<LeaderboardItemDisplay>();

    private int itemsToDisplay = 8;

    private void Awake()
    {
        leaderboardItems = new NetworkList<LeaderboardItemState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            if (ClientSingleton.Instance.GameManager.UserData.userGamePreferences.gameQueue == GameQueue.Team)
            {
                teamLeaderboard.SetActive(true);

                for(int i = 0; i < teamNames.Length; i++)
                {
                    LeaderboardItemDisplay teamLeaderboardItem = Instantiate(leaderboardItemPrefab, teamLeaderboardItemContent);
                    teamLeaderboardItem.Init(i, teamNames[i], 0);

                    Color teamColor = teamColorLookup.GetTeamColor(i);

                    teamLeaderboardItem.SetColor(teamColor);

                    teamItemDisplays.Add(teamLeaderboardItem);
                }
            }

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
        if (!gameObject.scene.isLoaded) return;

        switch(changeEvent.Type)
        {
            case (NetworkListEvent<LeaderboardItemState>.EventType.Add):

                if (!playerItemDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {

                    LeaderboardItemDisplay leaderboardItem = Instantiate(leaderboardItemPrefab, playerLeaderboardItemContent);

                    leaderboardItem.Init(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);

                    if(NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId)
                    {
                        leaderboardItem.SetColor(ownerColor);
                    }

                    playerItemDisplays.Add(leaderboardItem);

                }

                break;

            case (NetworkListEvent<LeaderboardItemState>.EventType.Remove):

                LeaderboardItemDisplay displayToRemove = playerItemDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToRemove == null) break;

                displayToRemove.transform.SetParent(null);
                playerItemDisplays.Remove(displayToRemove);
                Destroy(displayToRemove.gameObject);

                break;

            case (NetworkListEvent<LeaderboardItemState>.EventType.Value):

                LeaderboardItemDisplay displayToUpdate = playerItemDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

                if (displayToUpdate == null) break;

                displayToUpdate.UpdateCoins(changeEvent.Value.Coins);

                break;
        }

        playerItemDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

        for (int i = 0; i < playerItemDisplays.Count; i++)
        {
            playerItemDisplays[i].transform.SetSiblingIndex(i);
            playerItemDisplays[i].UpdateText();

            bool shouldShow = i <= itemsToDisplay - 1;
            playerItemDisplays[i].gameObject.SetActive(shouldShow);

        }

        LeaderboardItemDisplay localDisplay = playerItemDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if (localDisplay != null)
        {
            if (localDisplay.transform.GetSiblingIndex() >= itemsToDisplay)
            {
                playerLeaderboardItemContent.GetChild(itemsToDisplay - 1).gameObject.SetActive(false);
                localDisplay.gameObject.SetActive(true);
            }
        }

        if (!teamLeaderboard.activeSelf) return;

        LeaderboardItemDisplay teamDisplay = teamItemDisplays.FirstOrDefault(x => x.TeamIndex == changeEvent.Value.TeamIndex);

        if (teamDisplay != null)
        {
            if (changeEvent.Type == NetworkListEvent<LeaderboardItemState>.EventType.Remove)
            {
                teamDisplay.UpdateCoins(teamDisplay.Coins - changeEvent.Value.Coins);
            }
            else
            {
                teamDisplay.UpdateCoins(teamDisplay.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
            }

            teamItemDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

            for (int i = 0; i < teamItemDisplays.Count; i++)
            {
                teamItemDisplays[i].transform.SetSiblingIndex(i);
                teamItemDisplays[i].UpdateText();
            }
        }

    }

    private void HandlePlayerSpawned(PlayerTank player)
    {
        leaderboardItems.Add(new LeaderboardItemState
        {
            ClientId = player.OwnerClientId,
            TeamIndex = player.TeamIndex.Value,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (oldValue, newValue) => HandleCoinChanged(player.OwnerClientId, newValue);
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

        player.Wallet.TotalCoins.OnValueChanged -= (oldValue, newValue) => HandleCoinChanged(player.OwnerClientId, newValue);
    }

    private void HandleCoinChanged(ulong clientId, int coins)
    {
        for (int i = 0; i < leaderboardItems.Count; i++)
        {
            if (clientId != leaderboardItems[i].ClientId) continue;

            leaderboardItems[i] = new LeaderboardItemState
            {
                ClientId = leaderboardItems[i].ClientId,
                TeamIndex = leaderboardItems[i].TeamIndex,
                PlayerName = leaderboardItems[i].PlayerName.Value,
                Coins = coins
            };

            return;
        }
    }
}
