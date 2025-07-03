using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerTank : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] CinemachineCamera playerCam;
    [SerializeField] SpriteRenderer playerSprite;
    [SerializeField] Texture2D crosshair;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] int ownerPriority = 10;
    [SerializeField] Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<PlayerTank> OnPlayerSpawned;
    public static event Action<PlayerTank> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameData userData = null;

            if(IsHost)
            {
                userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);

            }
            else
            {
                userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);
            }

            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            playerCam.Priority = ownerPriority;
            playerSprite.color = ownerColor;

            //Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        OnPlayerDespawned?.Invoke(this);
    }
}
