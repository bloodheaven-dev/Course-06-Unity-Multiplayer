using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private PlayerCam player;
    [SerializeField] private TMP_Text displayPlayerName;

    private void Start()
    {
        OnPlayerNameChanged(string.Empty, player.PlayerName.Value);

        player.PlayerName.OnValueChanged += OnPlayerNameChanged;
    }

    private void OnDestroy()
    {
        player.PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        displayPlayerName.text = newName.ToString();
    }
}
