using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
    private bool isBusy;
    private float timeInQueue;

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

    private void Update()
    {
        if (isInQueue)
        {
            timeInQueue += Time.deltaTime;
            TimeSpan ts = TimeSpan.FromSeconds(timeInQueue);
            queueTime.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }
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
            isBusy = false;

            findGameText.text = "Find Game";
            queueStatus.text = string.Empty;
            queueTime.text = string.Empty;

            

            return;
        }

        if (isBusy) return;

        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);

        findGameText.text = "Cancel";
        queueStatus.text = "Searching...";
        timeInQueue = 0f;
        queueTime.text = "0:00";
        isInQueue = true;
        isBusy = true;
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
        if (isBusy) return;

        isBusy = true;

        await HostSingleton.Instance.GameManager.StartHostAsync();

        isBusy = false;
    }

    async void StartClientButton()
    {
        if (isBusy) return;

        isBusy = true;

        string joinCodeString = joinCodeField.text;
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeString);

        isBusy = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isBusy) return;

        isBusy = true;

        try
        {
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isBusy = false;
    }
}
