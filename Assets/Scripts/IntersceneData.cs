// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "IntersceneData", menuName = "Game Data/Interscene Data")]
public class IntersceneData : ScriptableObject
{
    ////////////////////////////
    // Interface set data
    [SerializeField] public List<SelectionData> CharacterList;
    [SerializeField] public List<SelectionData> HouseList;
    [SerializeField] public List<InventoryData> InventoryList;

    ////////////////////////////
    // Runtime character data
    public string CharacterName {get; set;}
    public SelectionData SelectedCharacter {get;set;}
    public SelectionData SelectedHouse {get;set;}

    // These are used when player data is loaded to set the correct selection data
    // for the game to use.
    public void SetSelectedCharacter(string NAME)
    {
        SelectedCharacter = (from entry in CharacterList where entry.NAME == NAME select entry).First();
    }

    public void SetSelectedHouse(string NAME)
    {
        SelectedHouse = (from entry in HouseList where entry.NAME == NAME select entry).First();
    }

    ////////////////////////////
    // Serialized data structures
    [Serializable]
    public class SelectionData
    {
        // selections like character, house, etc
        public SelectionData(string name, Sprite selectionSprite, Sprite ingameSprite)
        {
            NAME = name;
            SELECTION_SPRITE = selectionSprite;
            INGAME_SPRITE = ingameSprite;
        }
        [SerializeField] public string NAME;    // unique name for this selection
        [SerializeField] public Sprite SELECTION_SPRITE;  // sprite to display in the selection picker
        [SerializeField] public Sprite INGAME_SPRITE;     // sprite to display in game
    }


    [Serializable]
    public class InventoryData
    {
        public InventoryData(string name, string displayName, Sprite ingameSprite, Sprite iconSprite)
        {
            NAME = name;
            DISPLAY_NAME = displayName;
            INGAME_SPRITE = ingameSprite;
            ICON_SPRITE = iconSprite;
        }
        [SerializeField] public string NAME;            // unique name for this item
        [SerializeField] public string DISPLAY_NAME;    // name to display to user in UI
        [SerializeField] public Sprite INGAME_SPRITE;   // sprite to show user in game
        [SerializeField] public Sprite ICON_SPRITE;     // sprite to show user in UI
        [SerializeField] public bool WORLD_ITEM;        // item can go in world, otherwise it's clothing
    }
}
