// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrogJunction
{
    public class InventorySystem
    {
        public delegate void HandlePlaceInWorld(InventoryItem item);
        public delegate void HandleWear(InventoryItem item);
        public delegate void HandleUIUpdate();

        // JSON serialized item compatible with inventory service
        [Serializable]
        public class InventoryItem
        {
            public int uid;
            public string name;
            public bool inpocket;
            public bool wearable;
            public bool worn;
            public bool droppable;
            public bool dropped;
            public float x;
            public float y;

            public InventoryItem Clone()
            {
                return new InventoryItem()
                {
                    uid = this.uid,
                    name = String.Copy(this.name),
                    inpocket = this.inpocket,
                    wearable = this.wearable,
                    worn = this.worn,
                    droppable = this.droppable,
                    dropped = this.dropped,
                    x = this.x,
                    y = this.y
                };
            }

            public override bool Equals(object obj)
            {
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    InventoryItem item = (InventoryItem)obj;
                    return
                        (uid == item.uid) &&
                        (name == item.name) &&
                        (wearable == item.wearable) &&
                        (worn == item.worn) &&
                        (droppable == item.droppable) &&
                        (dropped == item.dropped) &&
                        (x == item.x) &&
                        (y == item.y);
                }
            }

            public override int GetHashCode()
            {
                return uid;
            }
        }


        public List<InventoryItem> Items { get; set; }

        public HandleUIUpdate UIUpdateHandler { private get; set; }

        public InventorySystem()
        {
            Items = new List<InventoryItem>();
        }

        public InventoryItem GetItem(int uid)
        {
            return Items.Find(x => x.uid == uid);
        }

        public void GetInventory(Action success, Action<string> failure)
        {
            APIRequest.Instance.GetRequest("inventory",
                (long code, string result) =>
                {
                    Items.Clear();
                    UnpackInventoryItemsFromJSON(result);
                    if (UIUpdateHandler != null)
                    {
                        UIUpdateHandler();
                    }
                    success();
                },
                (long code, string error) =>
                {
                    failure($"Error code: {code} error: {error}");
                });
        }

        public void SaveInventory(Action success, Action<string> failure)
        {
            foreach (InventoryItem item in Items)
            {
                var cachedItem = Array.Find(cachedItems.items, (x => x.uid == item.uid));
                if (cachedItem != null)
                {
                    if (!cachedItem.Equals(item))
                    {
                        APIRequest.Instance.PostRequest("inventory", item,
                        (long code, string result) =>
                        {
                            success();
                        },
                        (long code, string error) =>
                        {
                            failure($"Error code: {code} error: {error}");
                        });
                    }
                }
                else
                {
                    Debug.LogError($"Item found in local inventory not in cached id: {item.uid}");
                }
            }
            // this will cause the cache to reflect the current state of inventory
            // so only changes are flagged to the service.
            cachedItems.items = Items.ToArray();
        }

        [Serializable]
        private class InventoryTradeInfo
        {
            public int uid;
            public string newowner_id;

        }

        public void TradeInventory(int uid, string newOwner, Action success, Action<string> failure)
        {
            var tradeInfo = new InventoryTradeInfo()
            {
                uid = uid,
                newowner_id = newOwner
            };
            APIRequest.Instance.PostRequest("trade", tradeInfo,
            (long code, string result) =>
            {
                success();
                Items.Remove(GetItem(uid));
                UIUpdateHandler();
            },
            (long code, string error) =>
            {
                failure($"Error code: {code} error: {error}");
            });
        }

        [Serializable]
        private class PlayerListWrapper
        {
            public PlayerData[] Items;
        }
        public void RequestTradeList(Action<List<PlayerData>> success, Action<string> failure)
        {
            APIRequest.Instance.GetRequest("trade",
                (long code, string result) =>
                {
                    var players = JsonUtility.FromJson<PlayerListWrapper>(result).Items;
                    success(new List<PlayerData>(players));
                },
                (long code, string error) =>
                {
                    failure($"Error code: {code} error: {error}");
                });
        }

        public void UseItem(int uid, Vector3 placeVec, HandlePlaceInWorld placeInWorldHandler, HandleWear wearHandler)
        {
            var item = GetItem(uid);
            if (item.droppable)
            {
                UseWorldItem(item, placeVec, placeInWorldHandler);
            }
            else
            {
                WearItem(item, wearHandler);
            }
        }

        // if able to remove from the world, returns true
        public bool RemoveWorldItem(int uid)
        {
            var worldItem = GetItem(uid);
            if (worldItem != null)
            {
                if(worldItem.droppable)
                {
                    if(worldItem.dropped)
                    {
                        worldItem.inpocket = true;
                        worldItem.dropped = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Debug.LogError($"Attempting to drop non-world item uid:{uid} name: {worldItem.name}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool RemoveWornItem()
        {
            if(WornItemUID != -1)
            {
                var wornItem = GetItem(WornItemUID);
                wornItem.inpocket = true;
                wornItem.worn = false;
                WornItemUID = -1;
                return true;
            }
            return false;
        }

        public static InventorySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InventorySystem();
                }
                return _instance;
            }
        }
        private static InventorySystem _instance;


        private void UseWorldItem(InventoryItem item, Vector3 placeVec, HandlePlaceInWorld placeInWorldHandler)
        {
            item.inpocket = false;
            item.dropped = true;
            item.x = placeVec.x;
            item.y = placeVec.y;
            placeInWorldHandler(item);
        }

        private int WornItemUID = -1;

        private void WearItem(InventoryItem item, HandleWear wearHandler)
        {
            RemoveWornItem();
            WornItemUID = item.uid;
            item.worn = true;
            item.inpocket = false;
            wearHandler(item);
        }

        // This provides an object wrapper around the array for the
        // Unity JSON decoder.
        [Serializable]
        private class ItemListWrapper
        {
            public InventoryItem[] items;
        }

        private ItemListWrapper cachedItems;

        private void UnpackInventoryItemsFromJSON(string json)
        {
            // The Unity JSON API requires the top level item be an object
            cachedItems = JsonUtility.FromJson<ItemListWrapper>(json);
            foreach(var item in cachedItems.items)
            {
                var newItem = item.Clone();
                Items.Add(newItem);
                if(item.worn)
                {
                    WornItemUID = item.uid;
                }
            }
        }
    }
}
