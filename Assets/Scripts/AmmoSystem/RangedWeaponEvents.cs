﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RangedWeaponEvents : MonoBehaviour
{
    public static RangedWeaponEvents current;
    public event Action<RangedWeaponItemSO> onRangedWeaponEquipped;
    public event Action onRangedWeaponUnequipped;
    public event Action onInventoryHasChanged;
    public event Action<int> onRangedWeaponAmmoAmmountChange;

    private void Awake()
    {
        current = this;
    } 

    public void RangedWeaponAmmoAmountChange(int amount)
    {
        onRangedWeaponAmmoAmmountChange?.Invoke(amount);
    }    

    public void RangedWeaponEquipped(RangedWeaponItemSO rangedWeapon)
    {
        onRangedWeaponEquipped?.Invoke(rangedWeapon);
    }

    public void RangedWeaponUnequipped()
    {
        onRangedWeaponUnequipped?.Invoke();
    }

    public void InventoryHasChanged()
    {
        onInventoryHasChanged?.Invoke();
    }
}
