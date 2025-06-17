using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerTank : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineCamera playerCam;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] int ownerPriority = 10;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<PlayerTank> OnPlayerSpawned;
    public static event Action<PlayerTank> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);

            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            playerCam.Priority = ownerPriority;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        OnPlayerDespawned?.Invoke(this);
    }
}
