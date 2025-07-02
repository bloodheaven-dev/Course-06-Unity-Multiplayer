using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;

    private NetworkClient networkClient;
    private MatchplayMatchmaker matchplayMatchmaker;

    private GameData userData;

    private const string TITLE_SCREEN_STRING = "TitleScreen";

    public async Task<bool> InitAsync()
    {
        AuthenticationWrapper.ResetAuthState();

        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);
        matchplayMatchmaker = new MatchplayMatchmaker();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            userData = new GameData
            {
                userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };
            return true;
        }

        return false;
    }

    public async Task StartClientAsync(string joinCode)
    {

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
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
            allocation.HostConnectionData,
            allocation.Key,
            false
        );
        transport.SetRelayServerData(relayServerData);

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult matchmakingResult = await matchplayMatchmaker.Matchmake(userData);

        if(matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            // Connect to Server
        }

        return matchmakingResult.result;
    }

    public void Disconnect()
    {
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }


}
