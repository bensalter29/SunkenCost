﻿using System;
using System.Collections;
using System.Collections.Generic;
using BattleScreen;
using BattleScreen.BattleEvents;
using OfferScreen;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BattleHUDManager : MonoBehaviour, IBattleEventUpdatedUI
{
    [SerializeField] private Hearts _hearts;
    [SerializeField] private LoseLifeShaderController _loseLifeShaderController;
    
    [SerializeField] private TextMeshProUGUI _movesText;

    [SerializeField] private GoldDisplay _goldDisplay;

    [SerializeField] private NextTurnButton _nextTurnButton;
    [SerializeField] private EnemyTurnFrame _turnFrame;
    [SerializeField] private WhosTurnText _whosTurnText;

    private void Start()
    {
        BattleRenderer.Current.RegisterUIUpdater(this);
        
        UpdateLives();
        UpdateMovesText();
    }

    public void UpdateMovesText()
    {
        var movesLeft = (Player.Current.MovesPerTurn - Player.Current.MovesUsedThisTurn).ToString();
        _movesText.text = movesLeft + "/" + Player.Current.MovesPerTurn;
    }

    private void EndedBattle()
    {
        _nextTurnButton.UpdateText();
    }

    private void GainedLife()
    {
        UpdateLives();
    }

    private void LostLife()
    {
        UpdateLives();
        _loseLifeShaderController.PlayerLostLife();
    }

    private void UpdateLives()
    {
        _hearts.UpdateLives(Player.Current.Lives);
    }

    private void UpdateGoldText()
    {
        _goldDisplay.UpdateText(RunProgress.PlayerStats.Gold);
    }

    private void StartEnemyTurn()
    {
        _turnFrame.StartEnemyTurn();
        _whosTurnText.StartEnemyTurn();
    }

    private void EndEnemyTurn()
    {
        _turnFrame.EndEnemyTurn();
        _whosTurnText.EndEnemyTurn();
        UpdateMovesText();
    }

    private void PlankAddedOrRemoved()
    {
        ZoomManager.current.SetStickScale();
    }

    public void RespondToBattleEvent(BattleEvent battleEvent)
    {
        switch (battleEvent.type)
        {
            case BattleEventType.GainedGold:
                UpdateGoldText();
                break;
            case BattleEventType.PlayerGainedLife:
                GainedLife();
                break;
            case BattleEventType.PlayerLostLife:
                LostLife();
                break;
            case BattleEventType.PlayerUsedMove:
                UpdateMovesText();
                break;
            case BattleEventType.EndedBattle:
                EndedBattle();
                break;
            case BattleEventType.StartedNextTurn:
                StartEnemyTurn();
                break;
            case BattleEventType.EndedEnemyTurn:
                EndEnemyTurn();
                break;
            case BattleEventType.PlankCreated: case BattleEventType.PlankDestroyed:
                PlankAddedOrRemoved();
                break;
        }
    }
}
