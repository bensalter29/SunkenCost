﻿using BattleScreen;
using BattleScreen.BattleBoard;
using BattleScreen.BattleEvents;
using Enemies;

namespace Etchings
{
    public abstract class MovementActivatedEtching : Etching
    {
        protected override bool GetIfDesignIsRespondingToEvent(BattleEvent battleEvent)
        {
            if (stunned || battleEvent.type != GetEventType()) return false;

            var enemy = BattleEventsManager.Current.GetEnemyByResponderID(battleEvent.affectedResponderID);

            if (enemy.IsDestroyed) return false;
            
            // Enemy reached end
            if (enemy.PlankNum > Board.Current.PlankCount) return false;

            return ((design.Limitless || UsesUsedThisTurn < UsesPerTurn) && TestCharMovementActivatedEffect(enemy));
        }

        protected abstract BattleEventType GetEventType();
        
        protected abstract bool TestCharMovementActivatedEffect(Enemy enemy);
        
    }
}