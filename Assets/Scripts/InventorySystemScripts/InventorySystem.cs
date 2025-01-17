﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;
using System;
using SVS.InventorySystem;
using UnityEngine.EventSystems;

public class InventorySystem : MonoBehaviour, ISaveable
{
    [SerializeField] private int _playerStorageSize = 20;
    [SerializeField] InteractionManager _interactionManager;
    [SerializeField] private HotbarPanel _hotbarPanel;
    [SerializeField] private InventoryPanel _inventoryPanel;
    [SerializeField] private UIInventory _uIInventory;
    [SerializeField] private StructureItemSO _selectedStructureData = null;
    [SerializeField] private int _selectedStructureUIID = 0;
    private InventorySystemData _inventoryData;
    private Action _onInventoryStateChanged;
    private Action _onStructureUse;
    private DraggableItem _draggableItem;

    public int PlayerStorageSize { get => _playerStorageSize; }
    public Action OnInventoryStateChanged { get => _onInventoryStateChanged; set => _onInventoryStateChanged = value; }
    public bool WeaponEquipped { get => _inventoryData.ItemEquipped; }
    public string EquippedWeaponID { get => _inventoryData.EquippedItemID; }
    public Action OnStructureUse { get => _onStructureUse; set => _onStructureUse = value; }
    public StructureItemSO SelectedStructureData { get => _selectedStructureData; set => _selectedStructureData = value; }

    private void Awake()
    {
        _draggableItem = new DraggableItem();
    }

    private void Start()
    {
        _inventoryData = new InventorySystemData(_playerStorageSize, _hotbarPanel.HotbarElementsCount);
        _inventoryData.UpdateHotbarCallback += UpdateHotBarHandler;
        _uIInventory.AssignDropButtonHandler(DropHandler);
        _uIInventory.AssignUseButtonHandler(UseInventoryItemHandler);
        AddEventHandlersToHotbarUIElements();
        _onInventoryStateChanged += RangedWeaponEvents.current.InventoryHasChanged;
    }

    public void RemoveSelectedStructureFromInventory()
    {
        RemoveItemFromInventory(_selectedStructureUIID);
        _selectedStructureUIID = 0;
        _selectedStructureData = null;
    }

    private void UseInventoryItemHandler()
    {
        var selectedID = _inventoryData.SelectedItemUIID;
        var itemData = ItemDataManager.Instance.GetItemData(_inventoryData.GetItemIDFor(selectedID));
        UseItem(itemData, selectedID);
    }

    public void HotbarShortKeyHandler(int hotbarKey)
    {
        var ui_index = hotbarKey == 0 ? 9 : hotbarKey - 1;
        var uIElementID = _hotbarPanel.GetHotBarElementUIIDWithIndex(ui_index);
        if (uIElementID == -1) return;
        var id = _inventoryData.GetItemIDFor(uIElementID);
        if (id == null) return;
        var itemData = ItemDataManager.Instance.GetItemData(id);
        UseItem(itemData, uIElementID);
    }

    private void DropHandler()
    {
        var selectedID = _inventoryData.SelectedItemUIID;
        ItemSpawnManager.Instance.CreateItemAtPlayersFeet(_inventoryData.GetItemIDFor(selectedID), _inventoryData.GetItemCountFor(selectedID));
        ClearUIElement(selectedID);
        _inventoryData.RemoveItemFromInventory(selectedID);
        OnInventoryStateChanged.Invoke();
    }

    public bool CheckInventoryIsFull()
    {
        return _inventoryData.IsInventoryFull();
    }

    public void CraftAnItem(RecipeSO recipe)
    {
        foreach (var recipeIngredient in recipe.IngredientsRequired)
        {
            _inventoryData.TakeOneFromItem(recipeIngredient.Ingredients.ID, recipeIngredient.Count);
        }
        _inventoryData.AddToStorage(recipe);
        UpdateInventoryItems();
        UpdateHotBarHandler();
        OnInventoryStateChanged.Invoke();
    }

    public void RemoveAmmoItemCount(AmmoItemSO ammoItem, int amount)
    {
        _inventoryData.TakeOneFromItem(ammoItem.ID, amount);
        UpdateInventoryItems();
        UpdateHotBarHandler();
        OnInventoryStateChanged.Invoke();
    }

    private void UpdateInventoryItems()
    {
        ToggleInventory();
        ToggleInventory();
    }

    public bool CheckResourceAvailability(string id, int count)
    {
        return _inventoryData.IsItemInStorage(id, count);
    }

    public int ItemAmountInStorage(string id)
    {
        return _inventoryData.ItemAmountInStorage(id);
    }

    public ItemSO EquippedItem()
    {
        return ItemDataManager.Instance.GetItemData(EquippedWeaponID);
    }

    private void UseItem(ItemSO itemData, int ui_id)
    {
        if(itemData.GetItemType() == ItemType.Structure)
        {
            _selectedStructureUIID = ui_id;
            _selectedStructureData = (StructureItemSO)itemData;
            _onStructureUse.Invoke();
            return;
        }
        if(_interactionManager.UseItem(itemData))
        {
            RemoveItemFromInventory(ui_id);
        }
        else if(_interactionManager.EquipItem(itemData))
        {
            DeselectCurrentItem();
            ItemSpawnManager.Instance.RemoveItemFromPlayersBack();
            if (_inventoryData.ItemEquipped)
            {
                if (_inventoryData.EquippedUI_ID == ui_id)
                {
                    //Removes equipped item if user clicks use on already equipped item 
                    ToggleEquippedSelectedItemUI();
                    _inventoryData.UnequipItem();
                    RangeWeaponEventUnequip(itemData);
                    return;
                }
                else
                {
                    //Removes old equipped item if user equips another item
                    ToggleEquippedSelectedItemUI();
                    _inventoryData.UnequipItem();
                    RangeWeaponEventUnequip(itemData);
                }
            }
            //Adds newly equipped item 
            _inventoryData.EquipItem(ui_id);
            ToggleEquippedSelectedItemUI();
            ItemSpawnManager.Instance.CreateItemObjectOnPlayersBack(itemData.ID);
            RangedWeaponEvent(itemData);
        }
    }

    private void RemoveItemFromInventory(int ui_id)
    {
        _inventoryData.TakeOneFromItem(ui_id);
        if (_inventoryData.IsSelectedItemEmpty(ui_id))
        {
            ClearUIElement(ui_id);
            _inventoryData.RemoveItemFromInventory(ui_id);
        }
        else
        {
            UpdateUI(ui_id, _inventoryData.GetItemCountFor(ui_id));
        }
        OnInventoryStateChanged.Invoke();
    }

    private static void RangeWeaponEventUnequip(ItemSO itemData)
    {
        if (itemData.GetType() == typeof(RangedWeaponItemSO))
        {
            RangedWeaponEvents.current.RangedWeaponUnequipped();
        }
    }

    private void RangedWeaponEvent(ItemSO itemData)
    {
        if (itemData.GetType() == typeof(RangedWeaponItemSO))
        {
            RangedWeaponEvents.current.RangedWeaponEquipped((RangedWeaponItemSO)itemData);
        }
    }

    private void ToggleEquippedSelectedItemUI()
    {
        if (_hotbarPanel.IsItemInHotbarDictionary(_inventoryData.EquippedUI_ID))
        {
            _hotbarPanel.ToggleEquipSelectedItem(_inventoryData.EquippedUI_ID);
        }
        else if(_inventoryPanel.IsItemInInventoryDictionary(_inventoryData.EquippedUI_ID))
        {
            _inventoryPanel.ToggleEquipSelectedItem(_inventoryData.EquippedUI_ID);
        }
        else
        {
            throw new Exception("Selecteditem is not in Iventory or Hotbar " + _inventoryData.EquippedUI_ID);
        }
    }

    private void UpdateUI(int ui_id, int count)
    {
        if (_hotbarPanel.IsItemInHotbarDictionary(ui_id) == true)
        {
            _hotbarPanel.UpdateItemInfo(ui_id, count);
        }
        else
        {
            _inventoryPanel.UpdateItemInfo(ui_id, count);
        }
    }

    private void ClearUIElement(int ui_id)
    {
        _inventoryPanel.DeHighLightSelectedItem(ui_id);
        if (_hotbarPanel.IsItemInHotbarDictionary(ui_id) == true)
        {
            _hotbarPanel.ClearItemElement(ui_id);
        }
        else
        {
            _inventoryPanel.ClearItemElement(ui_id);
        }
        _uIInventory.ToggleItemButtons(false, false);
    }

    private void UpdateHotBarHandler()
    {
        var uIElements = _hotbarPanel.GetUIElementsForHotbar();
        var hotbarItemList = _inventoryData.GetItemDataForHotbar();
        for (int i = 0; i < uIElements.Count; i++)
        {
            var uIItemElement = uIElements[i];
            uIItemElement.ClearItem();
            var itemData = hotbarItemList[i];
            if (itemData.IsNull == false)
            {
                var itemName = ItemDataManager.Instance.GetItemName(itemData.ID);
                var itemSprite = ItemDataManager.Instance.GetItemSprite(itemData.ID);
                uIItemElement.SetItemUIElement(itemName, itemData.Count, itemSprite);
            }
        }
    }

    private void AddEventHandlersToHotbarUIElements()
    {
        var hotbarUIElements = _hotbarPanel.GetUIElementsForHotbar();
        for (int i = 0; i < hotbarUIElements.Count; i++)
        {
            _inventoryData.AddHotbarUIElement(hotbarUIElements[i].GetInstanceID());
            hotbarUIElements[i].OnClickEvent += UseHotBarItemHandler;
            hotbarUIElements[i].DragStartCallBack += UIElementBeginDragHandler;
            hotbarUIElements[i].DragContinueCallBack += UIElementContinueDragHandler;
            hotbarUIElements[i].DragStopCallBack += UIElementStopDragHandler;
            hotbarUIElements[i].DropCallBack += UIElementDropHandler;
        }
    }

    private void UseHotBarItemHandler(int ui_id, bool isEmpty)
    {
        if (isEmpty) return;
        DeselectCurrentItem();
        var itemData = ItemDataManager.Instance.GetItemData(_inventoryData.GetItemIDFor(ui_id));
        UseItem(itemData, ui_id);
    }

    public void ToggleInventory()
    {
        if(_uIInventory.IsInventoryVisable == false)
        {
            DeselectCurrentItem();
            _inventoryData.ClearInventoryUIElements();
            PrepareUI();
            PutDataInUI();
            _draggableItem.DestroyDraggedObject();
        }
        else
        {
            _draggableItem.DestroyDraggedObject();
        }
        _uIInventory.ToggleUI();
    }

    private void PutDataInUI()
    {
        var uIElements = _inventoryPanel.GetUIElementsForInventory();
        var inventoryItems = _inventoryData.GetItemsDataForInventory();
        for (int i = 0; i < uIElements.Count; i++)
        {
            var uIItemElement = uIElements[i];
            var itemData = inventoryItems[i];
            if(itemData.IsNull == false)
            {
                var itemName = ItemDataManager.Instance.GetItemName(itemData.ID);
                var itemSprite = ItemDataManager.Instance.GetItemSprite(itemData.ID);
                uIItemElement.SetItemUIElement(itemName, itemData.Count, itemSprite);
            }
            _inventoryData.AddInventoryUIElement(uIItemElement.GetInstanceID());
        }

        //refactor
        for (int i = 0; i < uIElements.Count; i++)
        {
            var uIItemElement = uIElements[i];
            if (_inventoryData.EquippedUI_ID == uIItemElement.GetInstanceID())
            {
                ToggleEquippedSelectedItemUI();
            }
        }
    }

    private void PrepareUI()
    {
        _inventoryPanel.DestoryAllItemsInInventory(_inventoryData.PlayerStorageLimit);
        _inventoryPanel.AddItemsToInventory(_inventoryData.PlayerStorageLimit);
        AddEventHandlersToInventoryUIElements();
    }

    private void AddEventHandlersToInventoryUIElements()
    {
        foreach(var uIItemElement in _inventoryPanel.GetUIElementsForInventory())
        {
            uIItemElement.OnClickEvent += UIElementSelectedHandler;
            uIItemElement.DragStartCallBack += UIElementBeginDragHandler;
            uIItemElement.DragContinueCallBack += UIElementContinueDragHandler;
            uIItemElement.DragStopCallBack += UIElementStopDragHandler;
            uIItemElement.DropCallBack += UIElementDropHandler;
        }
    }

    private void HandleUIItemFromHotbar(int droppedItemID, int draggedItemID)
    {
        if (_inventoryPanel.IsItemInInventoryDictionary(droppedItemID))
        {
            // item is swapping from hot bar to inventory
            DropItemsFromHotbarToInventory(droppedItemID, draggedItemID);
        }
        else
        {
            // item is swapping between hot bar to hot bar
            DropItemsFromHotbarToHotbar(droppedItemID, draggedItemID);
        }
    }

    private void DropItemsFromHotbarToHotbar(int droppedItemID, int draggedItemID)
    {
        _hotbarPanel.SwapUIHotbarItemToHotBarSlot(droppedItemID, draggedItemID);
        _draggableItem.DestroyDraggedObject();
        _inventoryData.SwapStorageItemsInsideHotbar(droppedItemID, draggedItemID);
        SetCurrentEquippedItemToDroppedItem(droppedItemID, draggedItemID);
    }

    private void DropItemsFromHotbarToInventory(int droppedItemID, int draggedItemID)
    {
        _hotbarPanel.SwapUIHotbarItemToInventorySlot(_inventoryPanel.InventoryUIItems, droppedItemID, draggedItemID);
        _draggableItem.DestroyDraggedObject();
        _inventoryData.SwapStorageItemFromHotbarToInventory(droppedItemID, draggedItemID);
        SetCurrentEquippedItemToDroppedItem(droppedItemID, draggedItemID);
    }


    private void HandleUIItemFromInventory(int droppedItemID, int draggedItemID)
    {
        if (_inventoryPanel.IsItemInInventoryDictionary(droppedItemID))
        {
            //item is from inventory
            DropItemsFromInventoryToInventory(droppedItemID, draggedItemID);
        }
        else
        {
            //item is from hotbar
            DropItemsFromInventoryToHotbar(droppedItemID, draggedItemID);
        }
    }

    private void DropItemsFromInventoryToHotbar(int droppedItemID, int draggedItemID)
    {
        _inventoryPanel.SwapUIInventoryItemToHotBarSlot(_hotbarPanel.HotbarUIItems, droppedItemID, draggedItemID);
        _draggableItem.DestroyDraggedObject();
        _inventoryData.SwapStorageItemFromInventoryToHotbar(droppedItemID, draggedItemID);
        SetCurrentEquippedItemToDroppedItem(droppedItemID, draggedItemID);
    }

    private void SetCurrentEquippedItemToDroppedItem(int droppedItemID, int draggedItemID)
    {
        if (_inventoryData.ItemEquipped && _inventoryData.EquippedUI_ID == draggedItemID)
        {
            DeselectCurrentItem();
            ToggleEquippedSelectedItemUI();
            _inventoryData.UnequipItem();
            _inventoryData.EquipItem(droppedItemID);
            ToggleEquippedSelectedItemUI();
        }
    }

    private void DropItemsFromInventoryToInventory(int droppedItemID, int draggedItemID)
    {
        _inventoryPanel.SwapUIInventoryItemToInventorySlot(droppedItemID, draggedItemID);
        _draggableItem.DestroyDraggedObject();
        _inventoryData.SwapStorageItemsInsideInventory(droppedItemID, draggedItemID);
        SetCurrentEquippedItemToDroppedItem(droppedItemID, draggedItemID);
    }

    private void UIElementDropHandler(PointerEventData eventData, int droppedItemID)
    {
        if(_draggableItem.DragItem != null)
        {
            var draggedItemID = _draggableItem.DraggableItemPanel.GetInstanceID();
            if (draggedItemID == droppedItemID)
                return;
            DeselectCurrentItem();
            if (_inventoryPanel.IsItemInInventoryDictionary(draggedItemID)) //if item is coming from the iventory to the hotbar
            {
                HandleUIItemFromInventory(droppedItemID, draggedItemID);

            }
            else //if item is coming from the hot bar to the inventory
            {
                HandleUIItemFromHotbar(droppedItemID, draggedItemID);
            }
        }
    }

    private void UIElementStopDragHandler(PointerEventData eventData)
    {
        _draggableItem.DestroyDraggedObject();
    }

    private void UIElementContinueDragHandler(PointerEventData eventData)
    {
        _draggableItem.MoveDraggableItem(eventData, _uIInventory.Canvas);
    }

    private void UIElementBeginDragHandler(PointerEventData eventData, int ui_id)
    {
        _draggableItem.DestroyDraggedObject();
        if (_inventoryPanel.IsItemInInventoryDictionary(ui_id))
        {
            _draggableItem.CreateDraggableItem(_inventoryPanel.InventoryUIItems[ui_id], _uIInventory.Canvas);
        }
        else
        {
            _draggableItem.CreateDraggableItem(_hotbarPanel.HotbarUIItems[ui_id], _uIInventory.Canvas);
        }
    }

    private void UIElementSelectedHandler(int ui_id, bool isEmpty)
    {
        if (isEmpty == false)
        {
            DeselectCurrentItem();
            _inventoryData.SetSelectedItem(ui_id);
            _inventoryPanel.HighLightSelectedItem(ui_id);
            _uIInventory.ToggleItemButtons(ItemDataManager.Instance.IsItemUsabel(_inventoryData.GetItemIDFor(_inventoryData.SelectedItemUIID)), true);
            if(_inventoryData.ItemEquipped)
            {
                if(ui_id == _inventoryData.EquippedUI_ID)
                {
                    _uIInventory.ToggleItemButtons(ItemDataManager.Instance.IsItemUsabel(_inventoryData.GetItemIDFor(_inventoryData.SelectedItemUIID)), false);
                }
            }
        }
        return;
    }

    private void DeselectCurrentItem()
    {
        if(_inventoryData.SelectedItemUIID != -1)
        {
            _inventoryPanel.DeHighLightSelectedItem(_inventoryData.SelectedItemUIID);
            _uIInventory.ToggleItemButtons(false, false);
        }
        _inventoryData.ResetSelectedItem();
    }

    public int AddToStorage(IInventoryItem item)
    {
        int value = _inventoryData.AddToStorage(item);
        OnInventoryStateChanged.Invoke();
        return value;
    }

    public string GetJsonDataToSave()
    {
        return JsonUtility.ToJson(_inventoryData.GetDataToSave());
    }

    public void LoadJsonData(string jsonData)
    {
        SavedItemSystemData dataToLoad = JsonUtility.FromJson<SavedItemSystemData>(jsonData);
        _inventoryData.LoadData(dataToLoad);
    }
}
