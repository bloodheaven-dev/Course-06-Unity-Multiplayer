using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class ServerSingleton : MonoBehaviour
{
    static ServerSingleton instance;
    public ServerGameManager GameManager { get; private set; }

    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ServerSingleton>();

            if (instance == null)
            {
                return null;
            }

            return instance;
        }

    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }


    public async Task CreateServer(NetworkObject playerPrefab)
    {
        await UnityServices.InitializeAsync();

        GameManager = new ServerGameManager
        (
            ApplicationData.IP(),
            ApplicationData.Port(),
            ApplicationData.QPort(),
            NetworkManager.Singleton,
            playerPrefab
        );
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }


}
