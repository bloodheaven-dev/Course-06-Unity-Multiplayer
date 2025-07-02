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

            await ClientSingleton.Instance.GameManager.CancelMatchmaking();

            isCancelling = false;
            isInQueue = false;

            findGameText.text = "Find Game";
            queueStatus.text = string.Empty;
            queueTime.text = string.Empty;

            return;
        }

        isInQueue = true;

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);

        findGameText.text = "Cancel";
        queueStatus.text = "Searching...";
        queueTime.text = "0:00";
    }

    private void OnMatchMade(MatchmakerPollingResult pollingResult)
    {
        switch(pollingResult)
        {
            case MatchmakerPollingResult.Success:

                queueStatus.text = "Connecting...";
                break;

            case MatchmakerPollingResult.TicketCreationError:

                queueStatus.text = "TicketCreationError";
                break;

            case MatchmakerPollingResult.TicketCancellationError:

                queueStatus.text = "TicketCancellationError";
                break;

            case MatchmakerPollingResult.TicketRetrievalError:

                queueStatus.text = "TicketRetrievalError";
                break;

            case MatchmakerPollingResult.MatchAssignmentError:

                queueStatus.text = "MatchAssignmentError";
                break;
        }
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
