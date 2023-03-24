using System;
using System.Collections.Generic;
using UnityEngine;

namespace Disturbances
{
    public class DisturbanceManager : MonoBehaviour
    {
        private static DisturbanceManager _current;
        private readonly Dictionary<DisturbanceType, DisturbanceAsset> _disturbances = new Dictionary<DisturbanceType, DisturbanceAsset>();
        private void Start()
        {
            _current = this;

            LoadDisturbanceAssets();
        }

        public static void LoadDisturbanceAssets()
        {
            _current._disturbances.Clear();
            var disturbances = Extensions.LoadScriptableObjects<DisturbanceAsset>();

            foreach (var disturbance in disturbances)
            {
                _current._disturbances.Add(disturbance.disturbanceType, disturbance);
            }
        }

        public static DisturbanceAsset GetDisturbance(DisturbanceType disturbanceType)
        {
            return _current._disturbances[disturbanceType];
        }

        public static void ExecuteEndOfBattleDisturbanceAction(Disturbance disturbance)
        {
            //TODO: FIX THIS

            switch (disturbance.DisturbanceType)
            {
                case DisturbanceType.GoldRush:
                    RunProgress.PlayerStats.AlterGold(disturbance.Modifier);
                    break;
                case DisturbanceType.Heart:
                    PlayerController.current.AddLife(disturbance.Modifier);
                    break;
                case DisturbanceType.None:
                    break;
                case DisturbanceType.UpgradeCard:
                    break;
                case DisturbanceType.Card:
                    case DisturbanceType.EliteCard:
                        if (!(disturbance is CardDisturbance cardDisturbance)) throw new Exception();
                        var rewardCard = cardDisturbance.Design;
                        rewardCard.MakeFree();
                        RunProgress.OfferStorage.RewardDesignOffers.Add(cardDisturbance.Design);
                    break;
                case DisturbanceType.Item:
                    case DisturbanceType.EliteItem:
                        if (!(disturbance is ItemDisturbance itemDisturbance)) throw new Exception();
                        RunProgress.ItemInventory.AddItem(itemDisturbance.ItemInstance);
                        break;
                case DisturbanceType.Move:
                    RunProgress.PlayerStats.MovesPerTurn++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static void ModifyDisturbanceAsset(DisturbanceType disturbanceType, int amount)
        {
            var disturbance = GetDisturbance(disturbanceType);

            disturbance.ModifyAmount(amount);
        }
    }
}
