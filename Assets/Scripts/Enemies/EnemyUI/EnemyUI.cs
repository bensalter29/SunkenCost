﻿using System;
using System.Collections;
using System.Collections.Generic;
using BattleScreen;
using BattleScreen.BattleEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies.EnemyUI
{
    public class EnemyUI : MonoBehaviour, IBattleEventUpdatedUI
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private TooltipTrigger _tooltipTrigger;
        
        [SerializeField] private EnemyTurnOrderText _turnOrderText;
        [SerializeField] private EnemyHealthText _healthText;
        [SerializeField] private EnemyMovementText _movementText;
        [SerializeField] private EnemyPoisonDisplay _poisonDisplay;
        [SerializeField] private EnemySpeechBubble _speechBubble;

        [SerializeField] private EnemyDestroyAnimation _destroyAnimation;

        private Enemy _enemy;
        private EnemyMover _mover;

        private void Awake()
        {
            BattleRenderer.Current.RegisterUIUpdater(this);
            _enemy = GetComponent<Enemy>();
            _tooltipTrigger.header = _enemy.Name;
            _mover = _enemy.Mover;
        }

        private void OnDestroy()
        {
            if (BattleRenderer.Current)
                BattleRenderer.Current.DeregisterUIUpdater(this);
        }

        public void RespondToBattleEvent(BattleEvent battleEvent)
        {
            if (battleEvent.type == BattleEventType.EnemyKilled &&
                battleEvent.affectedResponderID == _enemy.ResponderID)
            {
                _destroyAnimation.StartAnimation();
                return;
            }
            
            if (_enemy.IsDestroyed) return;
            if (battleEvent.type != BattleEventType.StartNextPlayerTurn
                && battleEvent.affectedResponderID != _enemy.ResponderID 
                && battleEvent.affectingResponderID != _enemy.ResponderID) return;

            if (battleEvent.type == BattleEventType.EnemySpeaking) _speechBubble.DisplayText(_enemy.Speech);

            _tooltipTrigger.content = _enemy.GetDescription();
            _turnOrderText.SetTurnOrder(_enemy.TurnOrder);
            _movementText.UpdateMovementText(_mover.CurrentMove.movementType, _mover.AmountOfMovesLeftThisTurn);
            _healthText.AlterHealth(_enemy.Health, _enemy.MaxHealth);
            _poisonDisplay.UpdateDisplay(_enemy.stats.Poison);
        }
    }
}