using Unity.Netcode;
using UnityEngine;

public class TerrainSpeed : NetworkBehaviour
{
    [SerializeField] float movementSpeedValue = 1.0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ModifyMovementSpeed(collision, -movementSpeedValue);
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        ModifyMovementSpeed(collision, movementSpeedValue);
    }

    void ModifyMovementSpeed(Collider2D collision, float movementSpeed)
    {
        if (!collision.attachedRigidbody.TryGetComponent<PlayerMovement>(out PlayerMovement playerMovement)) return;
        if (!playerMovement.IsOwner) return;

        playerMovement.ChangeMovementSpeedServerRpc(movementSpeed);
    }
}
