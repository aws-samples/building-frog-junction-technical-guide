// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject playerCharcater;
    public GameObject house;

    public IntersceneData intersceneData;

    // Start is called before the first frame update
    void Start()
    {
        var pcSprite = playerCharcater.GetComponent<SpriteRenderer>();
        pcSprite.sprite = intersceneData.SelectedCharacter.INGAME_SPRITE;

        var houseSprite = house.GetComponent<SpriteRenderer>();
        houseSprite.sprite = intersceneData.SelectedHouse.INGAME_SPRITE;
        
        var pcController = playerCharcater.GetComponent<PlayerCharacterController>();
        if(String.IsNullOrEmpty(intersceneData.CharacterName))
        {
            // put this in for testing when not launching right from the game scene
            intersceneData.CharacterName = "NAME";
        }
        pcController.SetName(intersceneData.CharacterName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
