using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerConnection : MonoBehaviour
{
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject clientButton;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideButtons();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideButtons();
    }

    void HideButtons()
    {
        startButton.SetActive(false);
        clientButton.SetActive(false);
    }
}
