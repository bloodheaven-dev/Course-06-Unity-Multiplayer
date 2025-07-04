using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] ClientSingleton clientPrefab;
    [SerializeField] HostSingleton hostPrefab;
    [SerializeField] ServerSingleton serverPrefab;
    [SerializeField] NetworkObject playerPrefab;

    private ApplicationData appData;

    private const string GAME_SCENE_STRING = "MainLevel";

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }


    async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            Application.targetFrameRate = 60;

            appData = new ApplicationData();

            ServerSingleton serverSingleton = Instantiate(serverPrefab);

            StartCoroutine(LoadSceneAsync(serverSingleton));

        }
        else
        {
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost(playerPrefab);

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool isAuthenticated = await clientSingleton.CreateClient();



            if(isAuthenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }

        }


    }

    private IEnumerator LoadSceneAsync(ServerSingleton serverSingleton)
    {


        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GAME_SCENE_STRING);
        while(!asyncOperation.isDone)
        {
            yield return null;
        }

        Task createServer = serverSingleton.CreateServer(playerPrefab);
        yield return new WaitUntil(() => createServer.IsCompleted);

        Task startServer = serverSingleton.GameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServer.IsCompleted);
    }

}
