﻿using Designs;

namespace Etchings
{
    public class PoisonEtching : CharMovementActivatedEtching
    {
        private Stat _poisonAmountStat;
        private int _poisonAmount => _poisonAmountStat.Value;

        public int initialPoisonAmount;

        protected override void Start()
        {
            _poisonAmountStat = new Stat(design.GetStat(StatType.Poison));
        
            base.Start();
        }

        protected override bool TestCharMovementActivatedEffect()
        {
            var enemy = ActiveEnemiesManager.CurrentEnemy;

            if (!CheckInfluence(enemy.StickNum)) return false;
            
            enemy.stats.AddPoison(_poisonAmount);
            enemy.Plank.SetTempColour(design.Color);
            InGameSfxManager.current.Poisoned();
            UsesUsedThisTurn++;
            return true;
        }
        
        protected override bool CheckInfluence(int stickNum)
        {
            return stickNum == Plank.GetPlankNum();
        }
    }
}
