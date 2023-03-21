using System.Collections.Generic;
using Disturbances;
using Items;
using OfferScreen;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MapScreen
{
    public class DisturbanceGenerator : MonoBehaviour
    {
        [SerializeField] private DisturbanceEvent topDisturbanceEvent;
        [SerializeField] private DisturbanceEvent bottomDisturbanceEvent;
    
    
        private static readonly Dictionary<DisturbanceType, float> NormalWeightings = new Dictionary<DisturbanceType, float>()
        {
            {DisturbanceType.GoldRush, 0.3f},
            {DisturbanceType.Heart, 0.2f},
            {DisturbanceType.UpgradeCard, 0.15f},
            {DisturbanceType.Move, 0.15f},
            {DisturbanceType.Card, 0.1f},
            {DisturbanceType.Item, 0.1f}
        };
    
        private static readonly Dictionary<DisturbanceType, float> EliteWeightings = new Dictionary<DisturbanceType, float>()
        {
            {DisturbanceType.EliteItem, 0.7f},
            {DisturbanceType.EliteCard, 0.3f}
        };

        private void Awake()
        {
            if (RunProgress.HasGeneratedMapEvents)
            {
                topDisturbanceEvent.Init(RunProgress.GeneratedMapDisturbances[0]);
                bottomDisturbanceEvent.Init(RunProgress.GeneratedMapDisturbances[1]);
                
                return;
            }
            
            var eliteRound = RunProgress.BattleNumber % 3 == 2;
        
            var weightings = eliteRound
                ? EliteWeightings
                : NormalWeightings;
            
            var generatedDisturbances = new List<Disturbance>();
        
            var topDisturbance = GenerateDisturbance(weightings);
            topDisturbanceEvent.Init(topDisturbance);
            generatedDisturbances.Add(topDisturbance);

            for (var i = 0; i < 1000; i++)
            {
                if (i == 999)
                {
                    Debug.Log("Couldn't generate a disturbance!!");
                }
            
                var bottomDisturbance = GenerateDisturbance(weightings);
                if (bottomDisturbance.DisturbanceType == topDisturbance.DisturbanceType) continue;
            
                bottomDisturbanceEvent.Init(bottomDisturbance);
                generatedDisturbances.Add(bottomDisturbance);
                break;
            }
            
            RunProgress.HaveGeneratedDisturbanceEvents(generatedDisturbances);
        }
        

        private static Disturbance GenerateDisturbance(Dictionary<DisturbanceType, float> weightings)
        {

            var rand = Random.value;

            foreach (var kvp in weightings)
            {
                var weighting = kvp.Value;

                if (rand <= weighting)
                {
                    var disturbanceAsset = DisturbanceManager.GetDisturbance(kvp.Key);

                    switch (disturbanceAsset.disturbanceType)
                    {
                        case DisturbanceType.Item:
                        case DisturbanceType.EliteItem:
                            return GenerateNewItemDisturbance(disturbanceAsset);
                        case DisturbanceType.Card:
                        case DisturbanceType.EliteCard:
                            return GenerateNewCardDisturbance(disturbanceAsset);
                        default:
                            return new Disturbance(disturbanceAsset, disturbanceAsset.amount);
                    }
                }

                rand -= weighting;
            }

            Debug.Log("Error: no weighting found for " + rand);
            return null;
        }

        private static ItemDisturbance GenerateNewItemDisturbance(DisturbanceAsset disturbanceAsset)
        {
            var itemsToExclude = RunProgress.ItemInventory.ItemAssets;
        
            var randomItemAsset = disturbanceAsset.disturbanceType == DisturbanceType.EliteItem
                ? ItemLoader.EliteItemAssets.GetRandomNonDuplicate(itemsToExclude)
                : ItemLoader.ShopItemAssets.GetRandomNonDuplicate
                    (itemsToExclude, ia => ia.rarity == ItemRarity.Common);

            var item = new ItemInstance(randomItemAsset, randomItemAsset.modifier);
                    
            return new ItemDisturbance(disturbanceAsset,disturbanceAsset.amount, item);
        }

        private static CardDisturbance GenerateNewCardDisturbance(DisturbanceAsset disturbanceAsset)
        {
            var design = disturbanceAsset.disturbanceType == DisturbanceType.EliteCard 
                ? DesignFactory.GenerateRandomRareDesign() : DesignFactory.GenerateStoreDesign();
            
            design.LevelUp();
            
            return new CardDisturbance(disturbanceAsset, disturbanceAsset.amount, design);
        }
    }
}
