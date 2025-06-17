using TMPro;
using Unity.Collections;
using UnityEngine;

public class LeaderboardItemDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text leaderboardItemText;

    private FixedString32Bytes playerName;

    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Init(ulong clientId, FixedString32Bytes playerName, int coin)
    {
        this.playerName = playerName;

        ClientId = clientId;
        Coins = coin;

        UpdateCoins(coin);
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;

        UpdateText();
    }

    private void UpdateText()
    {
        leaderboardItemText.text = $"1. {playerName} ({Coins})";
    }
}
