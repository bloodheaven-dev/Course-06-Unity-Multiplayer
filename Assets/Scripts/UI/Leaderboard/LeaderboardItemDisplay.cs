using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardItemDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardItemText;
    [SerializeField] private Color localColor;

    private FixedString32Bytes playerName;

    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Init(ulong clientId, FixedString32Bytes playerName, int coin)
    {
        this.playerName = playerName;

        ClientId = clientId;
        Coins = coin;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            leaderboardItemText.color = localColor;
        }

        UpdateCoins(coin);
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;

        UpdateText();
    }

    public void UpdateText()
    {
        leaderboardItemText.text = $"{transform.GetSiblingIndex() + 1} {playerName} ({Coins})";
    }
}
