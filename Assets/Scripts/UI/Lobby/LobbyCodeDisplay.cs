using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
public class LobbyCodeDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text lobbyCode;

    private HostGameManager GameManager;

    private NetworkVariable<FixedString32Bytes> LobbyCodeText = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        GameManager = HostSingleton.Instance?.GameManager;

        if (IsServer)
        {
            LobbyCodeText.Value = GameManager.JoinCode;
        }

        lobbyCode.text = LobbyCodeText.Value.ToString();
    }
}
