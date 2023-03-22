using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Challenges;
using Challenges.Challenges;
using Disturbances;
using Items;
using Items.Items;
using MapScreen;
using OfferScreen;
using UnityEngine;

public class RunProgress : MonoBehaviour
{
    private static RunProgress _current;
    private PlayerStats _playerStats;
    private OfferStorage _offerStorage;
    private ItemInventory _itemInventory;

    private int _battleNumber;

    private Disturbance _currentDisturbance;
    private List<Challenge> _activeChallenges;

    private bool _hasGeneratedMapEvents;

    public static PlayerStats PlayerStats => _current._playerStats;
    public static OfferStorage OfferStorage => _current._offerStorage;

    public static ItemInventory ItemInventory => _current._itemInventory;
    
    public static int BattleNumber => _current._battleNumber;

    public static Disturbance CurrentDisturbance => _current._currentDisturbance;
    
    public static List<Challenge> ActiveChallenges => _current._activeChallenges;

    public static bool HasGeneratedMapEvents => _current._hasGeneratedMapEvents;

    public static List<Disturbance> GeneratedMapDisturbances { get; private set; }

    public void Awake()
    {
        _current = this;

        _itemInventory = GetComponentInChildren<ItemInventory>();
    }

    public static void Initialise()
    {
        _current.InitialiseRun();
    }

    public static void SelectNextBattle(Disturbance disturbance)
    {
        _current._currentDisturbance = disturbance;
        _current._activeChallenges = ActiveChallenges.Where(c => c.IsActive).ToList();
        _current._battleNumber++;
        _current._hasGeneratedMapEvents = false;
    }

    public static void HaveGeneratedDisturbanceEvents(List<Disturbance> disturbances)
    {
        _current._hasGeneratedMapEvents = true;
        GeneratedMapDisturbances = disturbances;
    }

    public static Challenge[] ExtractCompletedChallenges()
    {
        var completedChallenges = ActiveChallenges.Where(c => c.HasAchievedCondition()).ToArray();

        foreach (var challenge in completedChallenges)
        {
            ActiveChallenges.Remove(challenge);
        }

        return completedChallenges;
    }
    
    private void InitialiseRun()
    {
        DisturbanceManager.LoadDisturbanceAssets();
        
        _playerStats = new PlayerStats();
        _playerStats.InitialiseDeck();
        _offerStorage = new OfferStorage();
        _itemInventory.WipeInventory();
        _battleNumber = 0;
        _currentDisturbance = null;
        _activeChallenges = new List<Challenge>();
        
        AddItem(typeof(ShortFuseItem));
    }

    // Used to test Items - add to initialise run
    private void AddItem(Type t)
    {
        var item = ItemLoader.ItemAssetToTypeDict.First
            (i => i.Value == t).Key;
        
        _itemInventory.AddItem(new ItemInstance(item, item.modifier));
    }
}
