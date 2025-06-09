using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    float xRange;
    float yRange;

    void Awake()
    {
        SetPosition();
    }

    void SetPosition()
    {
        xRange = Random.Range(-5, 5);
        yRange = Random.Range(-5, 5);

        transform.position = new Vector3(xRange, yRange, transform.position.z);
    }
}
