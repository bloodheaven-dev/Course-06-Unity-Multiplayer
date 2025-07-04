using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
public class LobbyCodeDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;

    private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (HostSingleton.Instance == null) return;

        if(IsHost)
        {
            lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
        }

        lobbyCodeText.text = lobbyCode.Value.ToString();

        if (string.IsNullOrEmpty(lobbyCodeText.text))
        {
            lobbyCodeText.transform.parent.gameObject.SetActive(false);
        }
    }
}
