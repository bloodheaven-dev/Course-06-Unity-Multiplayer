using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSingleton : MonoBehaviour
{
    static ClientSingleton instance;

    public ClientGameManager GameManager { get; private set; }

    const string TITLE_SCREEN_STRING = "TitleScreen";

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ClientSingleton>();

            if (instance == null)
            {
                Debug.LogError("ClientSingleton not in Scene!");
                return null;
            }

            return instance;
        }

    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();

        return await GameManager.InitAsync();
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(TITLE_SCREEN_STRING);
    }

}
