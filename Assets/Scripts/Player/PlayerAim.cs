using Unity.Netcode;
using UnityEngine;

public class PlayerAim : NetworkBehaviour
{
    [SerializeField] InputReader inputReader;
    [SerializeField] Transform turretTransform;

    void LateUpdate()
    {
        if (!IsOwner) return;

        Vector2 turretPosition = turretTransform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        Vector2 finalPosition =  mousePosition - turretPosition;

        turretTransform.up = new Vector2(finalPosition.x, finalPosition.y);
        
    }
}
