using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;

        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
