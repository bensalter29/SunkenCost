﻿using System;
using System.Linq;
using BattleScreen;
using BattleScreen.BattleBoard;
using BattleScreen.BattleEvents;
using Damage;

namespace Enemies
{
    public class CurrentEnemy
    {
        private readonly Enemy _enemy;
        private readonly EnemyMover _mover;
        private readonly int _responderID;
        
        private bool _hasDealtPoison;
        private bool _hasExecutedStartOfTurnAbility;
        private bool _yetToExecuteEndOfTurnAbility;

        private bool _hasToldEveryoneImAboutToMove;

        public CurrentEnemy(Enemy enemy)
        {
            _enemy = enemy;
            _mover = enemy.Mover;
            _responderID = enemy.ResponderID;
        }

        public BattleEventPackage GetNextAction()
        {
            if (_enemy.IsDestroyed) return new BattleEventPackage(CreateEvent(BattleEventType.EndedIndividualEnemyTurn));
            
            if (!_hasDealtPoison)
            {
                _hasDealtPoison = true;

                if (_enemy.stats.Poison > 0)
                    return new BattleEventPackage(_enemy.DealPoisonDamage())
                        .WithIdentifier(BattleEventType.EnemyEffect, _responderID);
            }
            
            if (!_hasExecutedStartOfTurnAbility)
            {
                _hasExecutedStartOfTurnAbility = true;

                if (_enemy is IStartOfTurnAbilityHolder startOfTurnAbilityHolder 
                    && startOfTurnAbilityHolder.GetIfUsingStartOfTurnAbility())
                {
                    // TODO: Battle event packages should be mergeable
                    var abilityEvents = (startOfTurnAbilityHolder.GetStartOfTurnAbility()).battleEvents.ToList();
                    return new BattleEventPackage(abilityEvents)
                        .WithIdentifier(BattleEventType.EnemyEffect, _responderID);
                }
            }

            if (!_mover.FinishedMoving)
            {
                // Tell everyone I'm about to move before each move
                if (!_hasToldEveryoneImAboutToMove)
                {
                    _hasToldEveryoneImAboutToMove = true;
                    return new BattleEventPackage(CreateEvent(BattleEventType.EnemyAboutToMove));
                }

                // Will always attack boat if on the last plank
                if (_mover.WouldMoveOntoBoat)
                {
                    _mover.AttackBoat();
                    var boatDamageEvent = CreateEvent(BattleEventType.EnemyAttackedBoat, _enemy.GetBoatDamage());
                    return new BattleEventPackage(boatDamageEvent);
                }
                
                _hasToldEveryoneImAboutToMove = false;

                // Change my plank
                _mover.MoveToNextPlank();
                
                // I've either moved a plank or reached the boat and died
                return new BattleEventPackage(CreateEvent(BattleEventType.EnemyMove));
            }
            
            //TODO: Add EndOfTurnAbility processing here
            
            
            return new BattleEventPackage(CreateEvent(BattleEventType.EndedIndividualEnemyTurn));
        }

        private BattleEvent CreateEvent(BattleEventType battleEventType, int modifier = 0)
        {
            return new BattleEvent(battleEventType) { primaryResponderID = _responderID, modifier = modifier};
        }
    }
}