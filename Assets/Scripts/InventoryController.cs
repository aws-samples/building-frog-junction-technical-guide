// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
using FrogJunction;

public class InventoryController : MonoBehaviour
{
    public IntersceneData intersceneData;
    public TradeDialog tradeDialog;
    public SpriteRenderer hat;
    public PlayerCharacterController player;
    public GameObject propPrefab;
    public List<UIInventoryItem> uiInventorySlots;

    private UIInventoryItem selectedItem;
    // quick lookup for inventory unique name to UI data
    private Dictionary<string, IntersceneData.InventoryData> inventoryUIMap;

    // Start is called before the first frame update
    void Start()
    {
        hat.gameObject.SetActive(false);
        inventoryUIMap = new Dictionary<string, IntersceneData.InventoryData>();
        foreach (var item in intersceneData.InventoryList)
        {
            inventoryUIMap.Add(item.NAME, item);
        }
        InventorySystem.Instance.UIUpdateHandler = this.UpdateInventoryUI;
        SetInitialInventoryState();
    }

    public void SelectItem(UIInventoryItem item)
    {
        // old item always unselected
        if(selectedItem != null) 
        {
            selectedItem.Select(false);
        }

        // if the currently selected item or an empty slot is clicked, deselected, otherwise
        // select the new item
        if(item == selectedItem || item.UID == -1)    
        {
            selectedItem = null;
        }
        else
        {
            selectedItem = item;
            selectedItem.Select(true);
        }
    }

    public void OnTradeItemClicked()
    {
        if(selectedItem != null && selectedItem.UID != -1)
        {
            tradeDialog.gameObject.SetActive(true);
            tradeDialog.PopulateList(selectedItem.UID);
        }
    }

    public void OnSaveClicked()
    {
        InventorySystem.Instance.SaveInventory(
            ()=> 
            {// no specific action needed here
            },
            (string error)=>
            {
                Debug.LogError($"Save failed: {error}");
            });
    }

    public void OnExitClicked()
    {
        OnSaveClicked();
    }

    public void OnUseItemClicked()
    {
        if(selectedItem != null)
        {
            InventorySystem.Instance.UseItem(selectedItem.UID, 
                player.GetPlaceItemPosition(),
                (worldItem) => 
                {
                    PlaceItemInWorld(worldItem);
                },
                (inventoryItem) =>
                {
                    WearItem(inventoryItem);
                }
            );
            UpdateInventoryUI();
        }
    }

    private void SetInitialInventoryState()
    {
        ClearInventoryUI();
        int inventorySlotIndex = 0;
        foreach (var item in InventorySystem.Instance.Items)
        {
            if (item.inpocket)
            {
                var uiData = inventoryUIMap[item.name];
                uiInventorySlots[inventorySlotIndex].Set(item.uid, uiData.DISPLAY_NAME, uiData.ICON_SPRITE);
                inventorySlotIndex++;
            }
            else if(item.worn)
            {
                WearItem(item);
            }
            else if(item.dropped)
            {
                PlaceItemInWorld(item);
            }
        }
    }

    private void PlaceItemInWorld(InventorySystem.InventoryItem item)
    {
        var go = Instantiate(propPrefab, new Vector3(item.x, item.y), Quaternion.identity);
        var spriteRenderer = go.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = inventoryUIMap[item.name].INGAME_SPRITE;
        var polygonCollider = go.GetComponent<PolygonCollider2D>();
        polygonCollider.pathCount = spriteRenderer.sprite.GetPhysicsShapeCount();
        var path = new List<Vector2>();
        for (int pathIndex = 0; pathIndex < polygonCollider.pathCount; ++pathIndex)
        {
            path.Clear();
            spriteRenderer.sprite.GetPhysicsShape(pathIndex, path);
            polygonCollider.SetPath(pathIndex, path.ToArray());
        }
        var controller = go.GetComponent<PropController>();
        controller.UID = item.uid;
        controller.OnClick.AddListener(RemoveItemFromWorld);
    }

    private void WearItem(InventorySystem.InventoryItem item)
    {
        hat.gameObject.SetActive(true);
        hat.sprite = inventoryUIMap[item.name].INGAME_SPRITE;
    }

    public void RemoveHat()
    {
        if(InventorySystem.Instance.RemoveWornItem())
        {
            hat.gameObject.SetActive(false);
            UpdateInventoryUI();
        }
    }

    private void UpdateInventoryUI()
    {
        ClearInventoryUI();
        int inventorySlotIndex = 0;
        foreach(var item in InventorySystem.Instance.Items)
        {
            if(item.inpocket)
            {
                var uiData = inventoryUIMap[item.name];
                uiInventorySlots[inventorySlotIndex].Set(item.uid, uiData.DISPLAY_NAME, uiData.ICON_SPRITE);
                inventorySlotIndex++;
            }
        }
    }

    private void ClearInventoryUI()
    {
        selectedItem = null;
        foreach(var item in uiInventorySlots)
        {
            item.Clear();
        }
    }

    private void RemoveItemFromWorld(PropController controller)
    {
        if(InventorySystem.Instance.RemoveWorldItem(controller.UID))
        {
            Destroy(controller.gameObject);
            UpdateInventoryUI();
        }
    }
}
