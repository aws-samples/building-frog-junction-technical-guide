// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;

namespace FrogJunction
{
    // PlayerData stores data sent from the service
  
    [Serializable]
    public class PlayerData
    {
        public string PlayerID;
        public string name;
        public string model;
        public string house;
        public int score;
    }

    // CreatePlayerData is used to create a new player request
    // Notably, the client doesn't determine their level, the service does
    [Serializable]
    public class CreatePlayerData
    {
        public string name;
        public string model;
        public string house;
    }

    public class PlayerDataHandler
    {
        public static void SetInterscenePlayerData(IntersceneData intersceneData, PlayerData data)
        {
            intersceneData.CharacterName = data.name;
            intersceneData.SetSelectedCharacter(data.model);
            intersceneData.SetSelectedHouse(data.house);
        }
    }
}