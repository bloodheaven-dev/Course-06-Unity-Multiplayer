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

    private bool isInQueue;
    private TMP_Text findGameText;
    private Image findGameDisplay;

    void Start()
    {
        if (ClientSingleton.Instance == null) return;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        findGameButton.onClick.AddListener(FindGameButton);
        hostButton.onClick.AddListener(StartHostButton);
        joinButton.onClick.AddListener(StartClientButton);

        findGameText = findGameButton.GetComponentInChildren<TMP_Text>();
        findGameDisplay = findGameText.GetComponentInChildren<Image>();
        findGameDisplay.gameObject.SetActive(false);
    }

    void FindGameButton()
    {
        isInQueue = !isInQueue;
        
        if (isInQueue)
        {
            findGameText.text = "Cancel";
        }
        else
        {
            findGameText.text = "Find Game";
        }

        findGameDisplay.gameObject.SetActive(isInQueue);
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
