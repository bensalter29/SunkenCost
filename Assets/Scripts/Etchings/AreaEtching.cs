﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etchings
{
    public class AreaEtching : DamageEtching
    {
        protected override bool TestCharMovementActivatedEffect()
        {
            var stickNum = Plank.GetPlankNum();
            if (Math.Abs(ActiveEnemiesManager.CurrentEnemy.StickNum - stickNum) > MaxRange) return false;
            
            var stickNums = new List<int>();
            for (var i = stickNum - MaxRange; i <= stickNum + MaxRange && i < PlankMap.current.stickCount; i++)
            {
                if (i <= 0) 
                    i = 1;
                PlankMap.current.stickGrid.GetChild(i).GetComponent<Plank>().SetTempColour(design.Color);
                stickNums.Add(i);
            }

            var enemies = ActiveEnemiesManager.Current.GetEnemiesOnSticks(stickNums);
            
            if (enemies.Count == 0) return false;
            
            InGameSfxManager.current.DamagedEnemy();
            
            foreach (var enemy in enemies)
            {
                DamageEnemy(enemy);
            }
            UsesUsedThisTurn++;
            return true;
        }
        
        protected override bool CheckInfluence(int stickNum)
        {
            return Math.Abs(Plank.GetPlankNum() - stickNum) <= MaxRange;
        }
        
    }
}