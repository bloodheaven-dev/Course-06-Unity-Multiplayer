using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] ClientSingleton clientPrefab;
    [SerializeField] HostSingleton hostPrefab;
    [SerializeField] ServerSingleton serverPrefab;

    private ApplicationData appData;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }


    async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            appData = new ApplicationData();

            ServerSingleton serverSingleton = Instantiate(serverPrefab);
            await serverSingleton.CreateServer();

            await serverSingleton.GameManager.StartGameServerAsync();
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool isAuthenticated = await clientSingleton.CreateClient();



            if(isAuthenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }

        }


    }

}
