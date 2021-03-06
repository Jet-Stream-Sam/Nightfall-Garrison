﻿using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections;
using Sirenix.OdinInspector;

public class HealthBar : MonoBehaviour
{
    [Required][SerializeField] private PlayerMainController playerController;
    [SerializeField] private Image barFill;
    [SerializeField] private Image barDamage;
    [SerializeField] private TMP_Text healthValue;

    [SerializeField] private float barDamageFreezeTime = 0.5f;
    [SerializeField] private float barDamageShrinkSpeed = 1;
    private float barDamageTimer;
    private IEnumerator damagedTimer;

    private void Start()
    {
        SetHealth(playerController.currentHealth, playerController.maxHealth);
        barDamage.fillAmount = barFill.fillAmount;
        playerController.hasDamaged += InflictDamage;
    }
    private void InflictDamage(int damage)
    {
        if (damagedTimer != null)
            StopCoroutine(damagedTimer);
        damagedTimer = DamagedTimer();
        StartCoroutine(damagedTimer);
        SetHealth(playerController.currentHealth, playerController.maxHealth);
    }
    private void SetHealth(int health, int maxHealth)
    {
        barFill.fillAmount = GetNormalizedHealth(health, maxHealth);
        healthValue.text = health.ToString();
    }

    private float GetNormalizedHealth(int currentHealth, int maxHealth)
    {
        return (float)currentHealth / maxHealth;
    }

    private IEnumerator DamagedTimer()
    {
        barDamageTimer = barDamageFreezeTime;
        yield return new WaitForSeconds(barDamageTimer);
        while (true) 
        {

            if(barFill.fillAmount < barDamage.fillAmount)
            {
                barDamage.fillAmount -= barDamageShrinkSpeed * Time.deltaTime;
                yield return null;
            }
            else
            {
                break;
            }
            
        }

    }
    private void OnDestroy()
    {
        playerController.hasDamaged -= InflictDamage;
    }
}
