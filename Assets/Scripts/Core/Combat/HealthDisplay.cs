using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Health health;
    [SerializeField] Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        health.CurrentHealth.OnValueChanged += HandleHealth;
        HandleHealth(0, health.CurrentHealth.Value);
    }
    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;

        health.CurrentHealth.OnValueChanged -= HandleHealth;
    }


    void HandleHealth(int oldHealth, int newHealth)
    {
        float maxHealth = health.MaxHealth;

        healthBarImage.fillAmount = (float)newHealth / maxHealth;
    }
}
