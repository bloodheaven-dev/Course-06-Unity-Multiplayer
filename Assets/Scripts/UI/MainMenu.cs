using System;
using System.Threading.Tasks;
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
    [SerializeField] Toggle teamToggle;
    [SerializeField] Toggle privateToggle;

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
            await AbortGame();

            return;
        }

        if (isBusy) return;

        ClientSingleton.Instance.GameManager.MatchmakeAsync(teamToggle.isOn, OnMatchMade);

        findGameText.text = "Cancel";
        queueStatus.text = "Searching...";
        timeInQueue = 0f;
        queueTime.text = "0:00";
        isInQueue = true;
        isBusy = true;
    }

    private async Task AbortGame()
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
    }

    private void OnMatchMade(MatchmakerPollingResult pollingResult)
    {
        switch(pollingResult)
        {
            case MatchmakerPollingResult.Success:

                queueStatus.text = "Connecting...";
                break;

            case MatchmakerPollingResult.TicketCreationError:

                PollingResultWarning(pollingResult);
                break;

            case MatchmakerPollingResult.TicketCancellationError:

                PollingResultWarning(pollingResult);
                break;

            case MatchmakerPollingResult.TicketRetrievalError:

                PollingResultWarning(pollingResult);
                break;

            case MatchmakerPollingResult.MatchAssignmentError:

                PollingResultWarning(pollingResult);
                break;
        }
    }

    private void PollingResultWarning(MatchmakerPollingResult pollingResult)
    {
        _ = AbortGame();
        Debug.LogWarning("Following Error occured during Find Game: " + pollingResult);
    }

    async void StartHostButton()
    {
        if (isBusy) return;

        isBusy = true;

        await HostSingleton.Instance.GameManager.StartHostAsync(privateToggle.isOn);

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
