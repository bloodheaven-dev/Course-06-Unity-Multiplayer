using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] int damage = 5;

    ulong ownerClientId;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        if (projectile.TeamIndex != -1)
        {
            if (collision.attachedRigidbody.TryGetComponent<PlayerTank>(out PlayerTank player))
            {
                if (player.TeamIndex.Value == projectile.TeamIndex) return;
            }
        }

        collision.attachedRigidbody.TryGetComponent<Health>(out Health health);
        health?.TakeDamage(damage);
    }
}
