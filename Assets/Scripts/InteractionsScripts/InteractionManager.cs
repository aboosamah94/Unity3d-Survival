﻿using SVS.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] AgentController _playerController;

    private void Start()
    {
        _playerController = FindObjectOfType<AgentController>();
    }

    public bool UseItem(ItemSO itemData)
    {
        var itemType = itemData.GetItemType();
        switch (itemType)
        {
            case ItemType.None:
                throw new System.Exception("Item can't have itemtype of NONE");
            case ItemType.Food:
                FoodItemSO foodData = (FoodItemSO)itemData;
                _playerController.PlayerStat.AddToHealth(foodData.HealthBonus);
                _playerController.PlayerStat.AddToStamina(foodData.StaminaBonus);
                return true;
            case ItemType.Weapon:
                WeaponItemSO weaponData = (WeaponItemSO)itemData;
                Debug.Log("Equiping weapon");
                return false;
            default:
                Debug.Log("Can't use an item of type " + itemType.ToString());
                return false;
        }
        //return false;
    }

    public bool EquipItem(ItemSO itemData)
    {
        var itemType = itemData.GetItemType();
        switch (itemType)
        {
            case ItemType.None:
                throw new Exception("Item cannot be set to none");
            case ItemType.Weapon:
                return true;
            default:
                break;
        }
        return false;
    }
}
