using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] InputReader inputReader;
    [SerializeField] Transform tank;
    [SerializeField] ParticleSystem dustCloud;

    [Header("Settings")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float turnRate = 180f;
    [SerializeField] float dustCloudAmount = 10f;

    public NetworkVariable<float> MovementSpeed = new NetworkVariable<float>();

    private Rigidbody2D rb;
    private ParticleSystem.EmissionModule emissionModule;

    float defaultMovementSpeed;
    float minMovementSpeed;
    float maxMovementSpeed;

    Vector2 previousMovementInput;
    Vector3 previousPos;

    private const float PARTICLE_DISTANCE = 0.005f;

    void Awake()
    {
        defaultMovementSpeed = movementSpeed;
        minMovementSpeed = movementSpeed * 0.5f;
        maxMovementSpeed = movementSpeed * 1.5f;

        rb = GetComponent<Rigidbody2D>();
        emissionModule = dustCloud.GetComponent<ParticleSystem>().emission;
    }
    void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousMovementInput.x * -turnRate * Time.deltaTime;
        tank.transform.Rotate(0f, 0f, zRotation);
    }

    void FixedUpdate()
    {
        if ((previousPos - transform.position).sqrMagnitude > PARTICLE_DISTANCE)
        {
            emissionModule.rateOverTime = dustCloudAmount;
        }
        else
        {
            emissionModule.rateOverTime = 0f;
        }

        previousPos = transform.position;

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
