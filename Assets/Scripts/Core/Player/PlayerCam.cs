using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCam : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineCamera playerCam;

    [Header("Settings")]
    [SerializeField] int ownerPriority = 10;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);

            PlayerName.Value = userData.userName;
        }

        if (IsOwner)
        {
            playerCam.Priority = ownerPriority;
        }
    }
}
