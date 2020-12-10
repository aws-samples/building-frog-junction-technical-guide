// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using UnityEngine;

namespace FrogJunction
{
    public class PlayerDataSystem
    {
        public PlayerData CachedPlayerData
        {
            get
            {
                return _cachedPlayerData;
            }
        }

        public void CreatePlayerData(CreatePlayerData playerData, Action success, Action<string> failure)
        {
            APIRequest.Instance.PostRequest<CreatePlayerData>("player", playerData,
                (long code, string result) =>
                {
                    success();
                },
                (long code, string error) =>
                {
                    failure(error);
                });
        }

        public void GetPlayerData(Action success, Action<string> failure)
        {
            APIRequest.Instance.GetRequest("player",
                (long code, string result) => {
                    _cachedPlayerData = JsonUtility.FromJson<PlayerData>(result);
                    success();
                },
                (long code, string error) => {
                    failure(error);
                });
        }


        public static PlayerDataSystem Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new PlayerDataSystem();
                }
                return _instance;
            }
        }
        private static PlayerDataSystem _instance;

        private PlayerData _cachedPlayerData;

    }
}
