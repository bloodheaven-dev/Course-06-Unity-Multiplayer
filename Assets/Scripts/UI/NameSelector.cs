using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] Button connectButton;

    [SerializeField] int minNameLength = 2;
    [SerializeField] int maxNameLength = 16;

    private const string PLAYER_NAME_KEY = "PlayerName";

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        usernameInputField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, string.Empty);  
        HandleNameChange(usernameInputField.text);
    }

    private void OnEnable()
    {
        usernameInputField.onValueChanged.AddListener(HandleNameChange);
        connectButton.onClick.AddListener(ConnectButtonEvent);
    }

    private void OnDisable()
    {
        usernameInputField.onValueChanged.RemoveListener(HandleNameChange);
        connectButton.onClick.RemoveListener(ConnectButtonEvent);
    }

    private void HandleNameChange(string name)
    {
        int usernameLength = usernameInputField.text.Length;

        if (usernameLength > maxNameLength)
        {
            usernameInputField.text = name.Substring(0, maxNameLength);
        }

        connectButton.interactable = usernameLength >= minNameLength && usernameLength <= maxNameLength;
    }

    private void ConnectButtonEvent()
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, usernameInputField.text);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
