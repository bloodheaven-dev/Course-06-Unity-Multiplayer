using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] InputReader inputReader;
    [SerializeField] Transform tank;

    [Header("Settings")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float turnRate = 180f;

    public NetworkVariable<float> MovementSpeed = new NetworkVariable<float>();

    Rigidbody2D rb;
    float defaultMovementSpeed;
    float minMovementSpeed;
    float maxMovementSpeed;

    Vector2 previousMovementInput;

    void Awake()
    {
        defaultMovementSpeed = movementSpeed;
        minMovementSpeed = movementSpeed * 0.5f;
        maxMovementSpeed = movementSpeed * 1.5f;

        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousMovementInput.x * -turnRate * Time.deltaTime;
        tank.transform.Rotate(0f, 0f, zRotation);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = (Vector3)tank.transform.up * previousMovementInput.y * MovementSpeed.Value;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            MovementSpeed.Value = movementSpeed;
        }

        if (IsOwner)
        {
            inputReader.MoveEvent += HandleMove;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent -= HandleMove;
    }


    void HandleMove(Vector2 movementValue)
    {
        if (!IsOwner) return;

        previousMovementInput = movementValue;
    }

    [ServerRpc]
    public void ChangeMovementSpeedServerRpc(float movementValue)
    {
        MovementSpeed.Value += movementValue;
        MovementSpeed.Value = Mathf.Clamp(MovementSpeed.Value, minMovementSpeed, maxMovementSpeed);
    }
}
