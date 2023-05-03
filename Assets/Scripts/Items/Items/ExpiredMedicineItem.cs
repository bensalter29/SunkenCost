﻿using System.Collections;
using BattleScreen;
using BattleScreen.BattleEvents;
using Damage;

namespace Items.Items
{
    public class ExpiredMedicineItem : EquippedItem
    {
        protected override bool GetIfRespondingToBattleEvent(BattleEvent battleEvent)
        {
            return battleEvent.type == BattleEventType.EnemyHealed;
        }

        protected override BattleEventPackage GetResponse(BattleEvent battleEvent)
        {
            return new BattleEventPackage(DamageHandler.DamageEnemy
                (Amount, battleEvent.affectedResponderID, DamageSource.Item));
        }
    }
}