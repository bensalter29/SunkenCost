﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;
using Items;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Current;
    public readonly static Dictionary<string, Type> Items = new Dictionary<string, Type>();

    [SerializeField] private List<string> itemTypes = new List<string>();
    [SerializeField] private List<Sprite> itemSprites = new List<Sprite>();

    private void Awake()
    {
        if (Current)
        {
            Destroy(this.gameObject);
            return;
        }

        Current = this;
        DontDestroyOnLoad(gameObject);

        // Get the Etching Offers
        var itemsEnumerable =
            Assembly.GetAssembly(typeof(InGameItem)).GetTypes().Where(t => t.IsSubclassOf(typeof(InGameItem)))
                .Where(ty => !ty.IsAbstract);

        foreach (var type in itemsEnumerable)
        {
            // Remove the 'Items.' from the start and 'Item' from end of file name
            if (type.FullName != null)
            {
                var itemName = type.FullName.Remove(type.FullName.Length - 4, 4).Remove(0, 6);
                Items.Add(itemName, type);
            }
            else
            {
                Debug.Log("What?");
            }
        }
    }

    public static Sprite GetItemSprite(string item)
    {
        var index = Current.itemTypes.IndexOf(item);
        return Current.itemSprites[index];
    }

    public static string[] GetItemTitleAndDescription(string item)
    {
        var title = "";
        var desc = "";

        switch (item)
        {
            case "PoisonTips":
                title = "Poison Tips";
                desc = "Apply " + PoisonTipsItem.PoisonAmount +" poison every time an enemy is attacked";
                break;
            
            case "ExpiredMedicine":
                title = "Expired Medicine";
                desc = "Whenever an enemy heals, it takes " + ExpiredMedicineItem.DamageAmount + " damage";
                break;
            case "ReDress":
                title = "ReDress";
                desc = "Whenever an enemy reaches your boat, deal " + ReDressItem.DamageAmount + " damage to all enemies";
                break;
        }

        return new string[2] {title, desc};
    }

    public static string GetRandomItem()
    {
        return Current.itemTypes[Random.Range(0, Current.itemTypes.Count)];
    }
}
