using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager networkManager;

    private const string MENU_SCENE_NAME = "TitleScreen";

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnPlayerDisconnect;
    }

    private void OnPlayerDisconnect(ulong clientID)
    {
        if (clientID != 0 && clientID != networkManager.LocalClientId) return;

        Disconnect();
    }

    public void Disconnect()
    {
        if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME)
        {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnPlayerDisconnect;
        }
    }


}
