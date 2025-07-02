using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
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
    private MatchplayBackfiller backfiller;

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

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                await StartBackfill(matchmakerPayload);

                networkServer.OnUserJoined += UserJoined;
                networkServer.OnUserJoined += UserLeft;
            }
            else
            {
                Debug.LogWarning("Payload timed out");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        if(!networkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("Server could not be started.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_STRING, LoadSceneMode.Single);
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 8);

        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }

    }

    private void UserJoined(GameData user)
    {
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();

        if(!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }
    private void UserLeft(GameData user)
    {
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();

        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }

        if(backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }

    }

    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    public void Dispose()
    {
        networkServer.OnUserJoined -= UserJoined;
        networkServer.OnUserLeft -= UserLeft;

        backfiller?.Dispose();
        multiplayAllocationService?.Dispose();
        networkServer?.Dispose();
    }
}
