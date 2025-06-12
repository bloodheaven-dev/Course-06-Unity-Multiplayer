using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class NetworkServer
{
    private NetworkManager networkManager;

    private Dictionary<ulong, string> clientIDToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIDToUserData = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }


    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIDToAuth[request.ClientNetworkId] = userData.playerAuthID;
        authIDToUserData[userData.playerAuthID] = userData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }
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
}
