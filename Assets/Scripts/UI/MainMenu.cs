using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Button findGameButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinCodeField;
    [SerializeField] TMP_Text queueStatus;
    [SerializeField] TMP_Text queueTime;

    private bool isInQueue;
    private bool isCancelling;

    private TMP_Text findGameText;

    void Start()
    {
        if (ClientSingleton.Instance == null) return;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        findGameButton.onClick.AddListener(FindGameButton);
        hostButton.onClick.AddListener(StartHostButton);
        joinButton.onClick.AddListener(StartClientButton);

        findGameText = findGameButton.GetComponentInChildren<TMP_Text>();
    }

    private async void FindGameButton()
    {
        if (isCancelling) return;

        if (isInQueue)
        {
            queueStatus.text = "Cancelling...";

            isCancelling = true;

            //await Stop Queue

            isCancelling = false;
            isInQueue = false;

            findGameText.text = "Find Game";
            queueStatus.text = string.Empty;
            queueTime.text = string.Empty;

            return;
        }

        isInQueue = true;

        findGameText.text = "Cancel";
        queueStatus.text = "Searching...";
        queueTime.text = "0:00";
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
