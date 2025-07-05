using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardItemDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    private FixedString32Bytes displayName;

    public int TeamIndex {  get; private set; }
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Init(ulong clientId, FixedString32Bytes displayName, int coin)
    {
        this.displayName = displayName;

        ClientId = clientId;
        Coins = coin;


        UpdateCoins(coin);
    }

    public void Init(int teamIndex, FixedString32Bytes displayName, int coin)
    {
        this.displayName = displayName;

        TeamIndex = teamIndex;
        Coins = coin;


        UpdateCoins(coin);
    }

    public void SetColor(Color color)
    {
        displayText.color = color;
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;

        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1} {displayName} ({Coins})";
    }
}
