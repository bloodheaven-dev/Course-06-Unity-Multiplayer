using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, GameData> authIdToUserData = new Dictionary<string, GameData>();

    public Action<GameData> OnUserJoined;
    public Action<GameData> OnUserLeft;

    public Action<string> OnPlayerLeft;

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
    {
        this.networkManager = networkManager;
        this.playerPrefab = playerPrefab;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    public bool OpenConnection(string ip, int port)
    {
        UnityTransport transport = networkManager.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort)port);

        return networkManager.StartServer();
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        GameData userData = JsonUtility.FromJson<GameData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;

        OnUserJoined?.Invoke(userData);

        _ = SpawnPlayerDelayed(request.ClientNetworkId);

        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private async Task SpawnPlayerDelayed(ulong clientId)
    {
        await Task.Delay(250);

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPointPos(), Quaternion.identity);
        playerInstance.SpawnAsPlayerObject(clientId);

    }

    public GameData GetUserDataByClientID(ulong clientNetworkId)
    {
        if (clientIdToAuth.TryGetValue(clientNetworkId, out string data))
        {
            if (authIdToUserData.TryGetValue(data, out GameData userData))
            {
                return userData;
            }

            return null;
        }
        return null;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            OnUserLeft?.Invoke(authIdToUserData[authId]);
            authIdToUserData.Remove(authId);
            OnPlayerLeft.Invoke(authId);
        }
    }

    public void Dispose()
    {
        if (networkManager == null) { return; }

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnServerStarted -= OnNetworkReady;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
