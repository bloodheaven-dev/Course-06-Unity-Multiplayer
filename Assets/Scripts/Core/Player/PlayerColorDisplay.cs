using UnityEngine;

public class PlayerColorDisplay : MonoBehaviour
{
    [SerializeField] private PlayerTank player;
    [SerializeField] private TeamColorLookup teamColorLookup;
    [SerializeField] private SpriteRenderer[] playerSprites;
    
    private void Start()
    {
        HandleColorDisplayChanged(0, player.TeamIndex.Value);

        player.TeamIndex.OnValueChanged += HandleColorDisplayChanged;
    }

    private void HandleColorDisplayChanged(int oldValue, int newValue)
    {
        Color playerColor = teamColorLookup.GetTeamColor(newValue);

        foreach(SpriteRenderer sprite in playerSprites)
        {
            sprite.color = playerColor;
        }
    }

    private void OnDestroy()
    {
        player.TeamIndex.OnValueChanged -= HandleColorDisplayChanged;
    }
}
