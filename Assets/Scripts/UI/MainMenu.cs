using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;

    [SerializeField] TMP_InputField joinCodeField;
    void Start()
    {
        hostButton.onClick.AddListener(StartHostButton);
        joinButton.onClick.AddListener(StartClientButton);
    }

    async void StartHostButton()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    async void StartClientButton()
    {
        string joinCodeString = joinCodeField.text;
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeString);
    }
}
