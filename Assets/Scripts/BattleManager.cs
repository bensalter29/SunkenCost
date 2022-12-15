﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using Items;
using UnityEngine.SceneManagement;

public enum GameState
{
    PlayerTurn,
    EnemyTurn,
    OfferingDesigns
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Current;
    
    public GameState gameState = GameState.PlayerTurn;
    public const float AttackTime = 0.6f;
    public static readonly int NumberOfTurns = 5;
    
    private Random _random = new Random();

    public int Turn { get; private set; } = 0;

    private void Awake()
    {
        // One instance of static objects only
        if (Current)
        {
            Destroy(gameObject);
            return;
        }
        
        Current = this;
    }

    private void Start()
    {
        GlobalEvents.current.LoadedLevel();
        
        BattleEvents.Current.OnDesignOfferAccepted += DesignOfferAccepted;
        BattleEvents.Current.OnOfferDesigns += OfferingDesigns;
        BattleEvents.Current.OnBossKilled += BossKilled;
        
        foreach (var design in Deck.Designs)
        {
            StickManager.current.CreateStick();
            EtchingManager.current.CreateEtching(StickManager.current.GetStick(StickManager.current.stickCount-1), design);
        }
        
        BattleEvents.Current.BeginGame();
        Turn = 1;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //
        if (Input.GetKeyDown(KeyCode.X))
        {
            ItemManager.current.EquipItem("ExtraTurn");
            InventoryManager.current.AlterGold(50);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            StickManager.current.DestroyStick(1);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SkipToBoss();
        }

        // Begin Next turn
        if (Input.GetKeyDown(KeyCode.N))
        {
            TryNextTurn();
        }
        //

        if (gameState == GameState.EnemyTurn && EtchingManager.current.finishedProcessingEnemyMove && ActiveEnemiesManager.current.finishedProcessingEnemyTurn)
        {
            BattleEvents.Current.EndEnemyTurn();
            BeginPlayerTurn();
        }

    }

    public void Quit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Music.current.SelectSong(0);
    }

    public bool TryNextTurn()
    {
        // LATER - chane to just NoTurn
        if (gameState == GameState.PlayerTurn)
        {
            if (Turn > NumberOfTurns)
            {
                //Deck.Designs = EtchingManager.current.etchingOrder.Select(etching => etching.design).ToList();
                GameProgress.BattleNumber++;
                MainManager.Current.LoadNextOfferScreen();
            }
            else
            {
                BattleEvents.Current.EndPlayerTurn();
                WhoseTurnText.current.EnemiesTurn();
                NextTurnButton.Current.CanClick(false);
                InGameSfxManager.current.NextTurn();

                // Give the game a frame to catch up
                StartCoroutine(ProcessTurnChangeover());
            }

            return true;
        }
        
        return false;
    }

    private void BeginPlayerTurn()
    {
        gameState = GameState.PlayerTurn;
        InGameSfxManager.current.BeginTurn();
        WhoseTurnText.current.PlayersTurn();
            
        BattleEvents.Current.BeginPlayerTurn();
            
        NextTurnButton.Current.CanClick(true);
            
        Turn++;
    }

    private void OfferingDesigns()
    {
        gameState = GameState.OfferingDesigns;
    }

    private void DesignOfferAccepted()
    {
        gameState = GameState.PlayerTurn;
        BeginPlayerTurn();
    }

    private void BossKilled()
    {
        Music.current.SelectSong(1);
    }

    public void SkipToBoss()
    {
        var alterAmount = 16 / Turn;
        if (alterAmount > 10) alterAmount = 10;
        InventoryManager.current.AlterGold(alterAmount);
        Turn = 15;
    }

    public void OutOfLives()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Music.current.SelectSong(0);
    }

    private IEnumerator ProcessTurnChangeover()
    {
        yield return new WaitForSeconds(0.3f);
        gameState = GameState.EnemyTurn;
        BattleEvents.Current.BeginEnemyTurn();
    }
}