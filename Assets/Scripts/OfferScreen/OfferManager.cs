using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using OfferScreen;
using TMPro;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class OfferManager : MonoBehaviour
{
    [SerializeField] private GoldDisplay goldDisplay;
    [SerializeField] private ItemIconsDisplay itemIconsDisplay;
    
    public static OfferManager Current;

    private ItemOfferGenerator _itemOfferGenerator;
    private DesignCardOfferGenerator _designCardOfferGenerator;

    public List<DesignCard> AllDesignCards { get; private set; }
    
    public BuyerSeller BuyerSeller { get; private set; }


    public static int CardsOnTopRow => Current._designCardOfferGenerator.CardsInDeckRowCount;

    private void Awake()
    {
        if (Current)
        {
            Destroy(Current.gameObject);
        }

        Current = this;
        
        BuyerSeller = new BuyerSeller(RunProgress.PlayerStats.Gold, goldDisplay);

        _itemOfferGenerator = GetComponent<ItemOfferGenerator>();
        _designCardOfferGenerator = GetComponent<DesignCardOfferGenerator>();
    }

    private void Start()
    {
        _itemOfferGenerator.Initialise();
        AllDesignCards = _designCardOfferGenerator.CreateDesignCards();
        StartCoroutine(WaitForDesignCardsToInitialise());
    }

    public void BackToMap()
    {
        if (BuyerSeller.Gold < 0) return;
        if (CardsOnTopRow > RunProgress.PlayerStats.MaxPlanks) return;

        var deckRow = _designCardOfferGenerator.CardsInDeckRow;
        var offerRow = _designCardOfferGenerator.CardsInLeaveRow;
        var itemOfferDisplays = _itemOfferGenerator.ItemOfferDisplays;
        
        RunProgress.OfferStorage.StoreOffers(deckRow, offerRow, itemOfferDisplays);
        RunProgress.PlayerStats.AlterGold(-RunProgress.PlayerStats.Gold + BuyerSeller.Gold);
        MainManager.Current.LoadMap();
    }

    public void TryMerge(DesignCard cardBeingMerged, DesignCard cardBeingMergedInto)
    {
        var cost = cardBeingMergedInto.Design.Cost;
        if (BuyerSeller.Gold < cost) return;
        BuyerSeller.Buy(cost);

        AllDesignCards.Remove(cardBeingMerged);
        Destroy(cardBeingMerged.gameObject);
        cardBeingMergedInto.Design.LevelUp();
        OfferScreenEvents.Current.RefreshOffers();
    }

    public void BuyItem(ItemOffer itemOffer)
    {
        RunProgress.ItemInventory.AddItem(itemOffer.itemInstance);
        BuyerSeller.Buy(itemOffer.Cost);
        OfferScreenEvents.Current.RefreshOffers();
        itemIconsDisplay.AddItemToDisplay(itemOffer.itemInstance);
    }

    private IEnumerator WaitForDesignCardsToInitialise()
    {
        yield return 0;
        OfferScreenEvents.Current.RefreshOffers();
    }
}
