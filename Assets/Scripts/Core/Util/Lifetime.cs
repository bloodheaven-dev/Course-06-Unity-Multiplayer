using UnityEngine;

public class Lifetime : MonoBehaviour
{


    [SerializeField] float lifeTime = 1f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
