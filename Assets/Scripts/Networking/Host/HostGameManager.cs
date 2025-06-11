using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager
{
    public string JoinCode { get; private set; }
    private string lobbyID;

    private Allocation allocation;
    private NetworkServer networkServer;

    private const int MaxConnections = 8;
    private const float HeartbeatTime = 15;
    private const string GAME_SCENE_STRING = "MainLevel";

    public async Task StartHostAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize or authenticate: {e}");
            return;
        }


        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(JoinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData
        (
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.ConnectionData,
            allocation.Key,
            false
        );
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode", new DataObject
                    (
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Unnamed Lobby");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyID = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartBeat(HeartbeatTime));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        networkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name")
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_STRING, LoadSceneMode.Single);
    }


    IEnumerator HeartBeat(float waitSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitSeconds);
        while(true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return delay;
        }
    }
}
