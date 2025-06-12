using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCam : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineCamera playerCam;

    [Header("Settings")]
    [SerializeField] int ownerPriority = 10;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCam.Priority = ownerPriority;
        }
    }
}
