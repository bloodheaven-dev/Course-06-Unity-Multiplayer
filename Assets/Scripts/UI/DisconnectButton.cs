using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectButton : MonoBehaviour
{
    private Button leaveButton;

    private void Awake()
    {
        leaveButton = GetComponent<Button>();

        leaveButton.onClick.AddListener(LeaveGame);
    }

    private void OnDisable()
    {
        leaveButton.onClick.RemoveListener(LeaveGame);
    }

    private void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }
}
