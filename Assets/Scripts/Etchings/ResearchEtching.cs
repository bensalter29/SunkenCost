﻿using Designs;
using UnityEngine;

namespace Etchings
{
    public class ResearchEtching : CharMovementActivatedEtching
    {
        protected override bool CheckInfluence(int stickNum)
        {
            return stickNum == Plank.GetPlankNum();
        }

        protected override bool TestCharMovementActivatedEffect()
        {
            var enemy = ActiveEnemiesManager.CurrentEnemy;
            if (!CheckInfluence(enemy.StickNum)) return false;
            StartCoroutine(ColorForActivate());
            Plank.SetTempColour(design.Color);

            var amountToHeal = enemy.MaxHealth.Value - enemy.Health;
                
            enemy.Heal(amountToHeal);

            var timesMetRequirement = (int)Mathf.Floor(((float)amountToHeal / GetStatValue(StatType.IntRequirement)));
            var amountOfGoldToGive = timesMetRequirement * GetStatValue(StatType.Gold);
            BattleManager.Current.AlterGold(amountOfGoldToGive);

            return true;

        }
    }
}