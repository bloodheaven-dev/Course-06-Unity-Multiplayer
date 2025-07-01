using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ServerGameManager : IDisposable
{
    private const string GAME_SCENE_STRING = "MainLevel";

    private string serverIP;
    private int serverPort;
    private int queryPort;
    
    private NetworkServer networkServer;
    private MultiplayAllocationService multiplayAllocationService;

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        networkServer = new NetworkServer(manager);
        multiplayAllocationService = new MultiplayAllocationService();
    }

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        if(!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("Server could not be started.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_STRING, LoadSceneMode.Single);
    }

    public void Dispose()
    {
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
