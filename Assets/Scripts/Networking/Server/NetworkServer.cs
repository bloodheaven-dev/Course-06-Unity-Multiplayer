using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log(userData.userName);

        response.Approved = true;
        response.CreatePlayerObject = true;
    }
<<<<<<< HEAD
    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnPlayerDisconnect;
    }

    private void OnPlayerDisconnect(ulong clientID)
    {
        if(clientIDToAuth.TryGetValue(clientID, out string authID))
        {
            clientIDToAuth.Remove(clientID);

            authIDToUserData.Remove(authID);
        }
    }
    public void Dispose()
    {
        if (networkManager == null) return;

        networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        networkManager.OnServerStarted -= OnNetworkReady;
        networkManager.OnClientDisconnectCallback -= OnPlayerDisconnect;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
=======
>>>>>>> parent of e9fc704 (Handling Connections)
}
