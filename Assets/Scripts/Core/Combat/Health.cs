using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    bool isDead;
    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }
    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    void ModifyHealth(int healthValue)
    {
        if (isDead) return;

        int newHealth = CurrentHealth.Value + healthValue;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if (CurrentHealth.Value <= 0)
        {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
}
