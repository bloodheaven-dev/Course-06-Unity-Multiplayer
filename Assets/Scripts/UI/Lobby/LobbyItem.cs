using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] TMP_Text lobbyName;

    [SerializeField] TMP_Text lobbyCount;

    Lobby lobby;
    LobbiesList lobbiesList;


    public void Initialize(Lobby lobby, LobbiesList lobbiesList)
    {
        this.lobby = lobby;
        this.lobbiesList = lobbiesList;

        lobbyName.text = lobby.Name;
        lobbyCount.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

    }

    public void Join()
    {
        //Test
        lobbiesList.JoinAsync(lobby);
    }
}
