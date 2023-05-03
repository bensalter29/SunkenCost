﻿using System;
using System.Collections;
using BattleScreen;
using BattleScreen.BattleEvents;

namespace Items.Items
{
    public class PaciFistItem : EquippedItem
    {
        private bool _hasKilledEnemyThisBattle = false;

        protected override bool GetIfRespondingToBattleEvent(BattleEvent battleEvent)
        {
            return battleEvent.type == BattleEventType.EnemyKilled ||
                   battleEvent.type == BattleEventType.EndedBattle;
        }
        
        protected override BattleEventPackage GetResponse(BattleEvent battleEvent)
        {
            switch (battleEvent.type)
            {
                case BattleEventType.EnemyKilled:
                    _hasKilledEnemyThisBattle = true;
                    break;
                case BattleEventType.EndedBattle:
                    if (!_hasKilledEnemyThisBattle) return new BattleEventPackage(new BattleEvent(BattleEventType.TryGainedGold) {modifier = Amount});
                    else break;
                default:
                    throw new UnexpectedBattleEventException(battleEvent);
            }

            return BattleEventPackage.Empty;
        }
    }
}