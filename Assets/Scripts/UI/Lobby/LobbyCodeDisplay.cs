using TMPro;
using UnityEngine;

public class LobbyCodeDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text lobbyCode;

    HostGameManager GameManager;

    void Start()
    {
        GameManager = HostSingleton.Instance?.GameManager;

        if (GameManager != null)
        {
            lobbyCode.text = GameManager.JoinCode;
        }
        else
        {
            lobbyCode.text = "No Join Code";
            Debug.LogWarning("GameManager is null in LobbyCodeDisplay.");
        }
    }

}
