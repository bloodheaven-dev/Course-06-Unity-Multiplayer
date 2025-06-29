using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthBar;

    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private int costPerTick = 10;
    [SerializeField] private int healPerTick = 10;

    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private float healCooldown = 60f;

    private float remainingCooldown;
    private float tickTimer;

    private List<PlayerTank> playersInZone = new List<PlayerTank>();
    private NetworkVariable<float> HealPower = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }

        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPower;
            HandleHealPower(0, HealPower.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPower;
        }
    }

    private void HandleHealPower(float oldHealPower, float newHealPower)
    {
        healthBar.fillAmount = newHealPower / maxHealPower;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;

            float percentage = 1f - (remainingCooldown / healCooldown);

            HealPower.Value = maxHealPower * percentage;

            if (remainingCooldown <= 0f)
            {

                if (!ColorUtility.TryParseHtmlString("#42B243", out Color newColor)) return;

                healthBar.color = newColor;
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if(tickTimer >= 1 / healTickRate)
        {
            foreach(PlayerTank player in playersInZone)
            {
                if (HealPower.Value == 0) break;

                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;

                if (player.Wallet.TotalCoins.Value < costPerTick) continue;

                player.Wallet.SpendCoins(costPerTick);
                player.Health.RestoreHealth(healPerTick);

                HealPower.Value -= 1;

                if (HealPower.Value <= 0)
                {
                    if (!ColorUtility.TryParseHtmlString("#570000", out Color newColor)) return;
                    healthBar.color = newColor;

                    remainingCooldown = healCooldown;
                }

            }

            tickTimer = tickTimer % (1 / healTickRate);
        }


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<PlayerTank>(out PlayerTank player)) return;

        playersInZone.Add(player);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<PlayerTank>(out PlayerTank player)) return;

        playersInZone.Remove(player);
    }
}
