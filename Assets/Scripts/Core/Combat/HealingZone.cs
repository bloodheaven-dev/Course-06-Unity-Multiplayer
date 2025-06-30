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

    private Collider2D healingZone;
    private Coroutine healTickCoroutine;
    private float currentCooldown;
    private bool canHeal = true;

    private List<PlayerTank> playersInZone = new List<PlayerTank>();
    private NetworkVariable<float> HealPower = new NetworkVariable<float>();

    private void Awake()
    {
        healingZone = GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }

        if (IsClient)
        {
            HealPower.OnValueChanged += HandleDisplayHealPower;
            HandleDisplayHealPower(0, HealPower.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleDisplayHealPower;
        }
    }

    private void HandleDisplayHealPower(float oldHealPower, float newHealPower)
    {
        healthBar.fillAmount = newHealPower / maxHealPower;
    }


    private IEnumerator HealTickCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(healTickRate);

        while(canHeal)
        {
            if(currentCooldown <= 0f)
            {
                HealPlayer();
            }

            yield return wait;
        }

    }

    private void HealPlayer()
    {
        foreach (PlayerTank player in playersInZone)
        {
            if (HealPower.Value == 0) break;
            if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;
            if (player.Wallet.TotalCoins.Value < costPerTick) continue;

            player.Health.RestoreHealth(healPerTick);
            player.Wallet.SpendCoins(costPerTick);

            HealPower.Value -= 1;

            if (HealPower.Value == 0 && canHeal)
            {
                canHeal = false;

                healthBar.color = Color.darkRed;
                currentCooldown = healCooldown;
                StartCoroutine(CooldownCoroutine());

                break;
            }

        }
    }

    private IEnumerator CooldownCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(healTickRate / 4);

        while(currentCooldown > 0f)
        {
            currentCooldown -= healTickRate / 4;
            float percentage = 1f - (currentCooldown / healCooldown);
            HandleDisplayHealPower(0, maxHealPower * percentage);

            yield return wait;
        }

        healthBar.color = Color.green;
        HealPower.Value = maxHealPower;
        canHeal = true;

        healingZone.enabled = false;
        healingZone.enabled = true;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<PlayerTank>(out PlayerTank player)) return;


        playersInZone.Add(player);

        if (healTickCoroutine == null)
        {
            healTickCoroutine = StartCoroutine(HealTickCoroutine());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<PlayerTank>(out PlayerTank player)) return;

        playersInZone.Remove(player);

        if (playersInZone.Count > 0 || healTickCoroutine == null) return;

        StopCoroutine(healTickCoroutine);
        healTickCoroutine = null;
    }
}
