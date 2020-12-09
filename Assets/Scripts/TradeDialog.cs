// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using FrogJunction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeDialog : MonoBehaviour
{
    public ToggleGroup nameGroup;
    public List<Toggle> NameSlots;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        foreach(var nameSlot in NameSlots)
        {
            nameSlot.GetComponentInChildren<Text>().text = "";
        }
    }

    public void PopulateList(int currentSelectionUID)
    {
        UID = currentSelectionUID;
        InventorySystem.Instance.RequestTradeList(
        (List<PlayerData> playerList)=>
        {
            PlayerList = playerList;
            int cur = 0;
            foreach (var player in PlayerList)
            {
                NameSlots[cur].GetComponentInChildren<Text>().text = player.name;                
                ++cur;
                if(cur > NameSlots.Count)
                {
                    // Unity list box out of the scope of this demo :)
                    break;
                }
            }
        },
        (string errorMessage) =>
        {
            NameSlots[0].GetComponentInChildren<Text>().text = errorMessage;
        });
    }

    public void OnCancelButtonSelected()
    {
        gameObject.SetActive(false);
    }

    public void OnTradeButtonSelected()
    {
        int index = NameSlots.FindIndex(x => x.isOn == true);
        if(index != -1)
        {
            var player = PlayerList[index];
            InventorySystem.Instance.TradeInventory(UID, player.PlayerID,
                () =>
                {
                    OnCancelButtonSelected();
                },
                (string error) =>
                {
                    NameSlots[0].GetComponentInChildren<Text>().text = $"Trade error: {error}. Close trade window.";
                });
        }
    }

    private void ClearList()
    {
        foreach (var nameSlot in NameSlots)
        {
            nameSlot.GetComponentInChildren<Text>().text = "";
        }

    }

    private List<PlayerData> PlayerList;
    private int UID;

}
