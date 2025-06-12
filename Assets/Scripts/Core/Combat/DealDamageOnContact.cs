using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] int damage = 5;

    ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        if (collision.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            if (ownerClientId == networkObject.OwnerClientId) return;
        }

        collision.attachedRigidbody.TryGetComponent<Health>(out Health health);
        health?.TakeDamage(damage);
    }
}
