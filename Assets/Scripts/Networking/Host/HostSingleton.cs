using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{

    static HostSingleton instance;
    public HostGameManager GameManager { get; private set; }

    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<HostSingleton>();

            if (instance == null)
            {
                return null;
            }

            return instance;
        }

    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost(NetworkObject playerPrefab)
    {
        GameManager = new HostGameManager(playerPrefab);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

}
