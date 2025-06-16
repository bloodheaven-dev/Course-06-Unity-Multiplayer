using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] Transform leaderboardHolder;
    [SerializeField] LeaderboardContent leaderboardContentPrefab;

    private NetworkList<LeaderboardState> leaderboardEntities;

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardState>();
    }
}
