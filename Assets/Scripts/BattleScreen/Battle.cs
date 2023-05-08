﻿using System.Collections;
using System.Collections.Generic;
using BattleScene;
using BattleScreen.BattleEvents;
using Damage;
using Disturbances;
using UI;
using UnityEngine;

namespace BattleScreen
{
    public enum GameState
    {
        Loading,
        Paused,
        PlayerActionPeriod,
        ExecutingPlayerTurnEvents,
        EnemyTurn,
        Rewards,
        PlayerDied
    }
    
    public class Battle : MonoBehaviour
    {
        public const float ActionExecutionSpeed = 0.65f;
        
        public static Battle Current;

        private InGameSfxManager _sfxManager;
        private BattleRenderer _battleRenderer;

        [SerializeField] private EndOfBattlePopup _endOfBattlePopup;
        [SerializeField] private PlayerDeathPopup _playerDeathPopup;
        [SerializeField] private BattleHUDManager _hudManager;

        public int Turn { get; private set; } = 0;
        public GameState GameState { get; private set; } = GameState.Loading;
        
        private void Awake()
        {
            if (Current)
                Destroy(Current.gameObject);

            Current = this;
        }

        private void Start()
        {
            _sfxManager = InGameSfxManager.current;
            _battleRenderer = BattleRenderer.Current;

            foreach (var design in RunProgress.PlayerStats.Deck)
            {
                var plank = PlankFactory.Current.CreatePlank();
                EtchingFactory.Current.CreateEtching(plank, design);
            }
            
            // Give the game one frame to load etchings, enemies etc
            StartCoroutine(InitializeBattle());
        }

        public void ClickedNextTurn()
        {
            if (GameState != GameState.PlayerActionPeriod) return;
            StartCoroutine(NextEnemyTurn());
        }
        
        public void ClickedQuit()
        {
            throw new System.NotImplementedException();
        }

        public void InvokeResponsesToPlayerTurnEvent(BattleEvent battleEvent)
        {
            StartCoroutine(ExecutePlayerTurnEvents(battleEvent));
        }

        private IEnumerator InitializeBattle()
        {
            Debug.Log("------ Starting battle ------");
            // Gives other initialisers a clear frame to initialise
            yield return 0;
            yield return 0;

            var startBattle = new BattleEvent(BattleEventType.StartedBattle);
            yield return StartCoroutine(StartChainOfEvents(startBattle));
            yield return StartCoroutine(NextPlayerTurn());
        }
        
        private IEnumerator NextPlayerTurn()
        {
            Turn++;
            if (Turn <= RunProgress.PlayerStats.NumberOfTurns)
            {
                Debug.Log("------ PLAYERS TURN! ------");
                yield return StartCoroutine(StartChainOfEvents(new BattleEvent(BattleEventType.StartNextPlayerTurn)));
                GameState = GameState.PlayerActionPeriod;
            }
            else
            {
                yield return StartCoroutine(EndBattle());
            }
        }

        private IEnumerator ExecutePlayerTurnEvents(BattleEvent battleEvent)
        {
            GameState = GameState.ExecutingPlayerTurnEvents;
            
            // Give the board time to refresh if the player moved a plank
            if (battleEvent.type == BattleEventType.PlayerMovedPlank)
            {
                yield return 0;
            }

            yield return StartCoroutine(StartChainOfEvents(battleEvent));
            GameState = GameState.PlayerActionPeriod;
        }
        
        private IEnumerator NextEnemyTurn()
        {
            GameState = GameState.EnemyTurn;
            Debug.Log("------ Starting Enemy Turn ------");
            yield return StartCoroutine(StartChainOfEvents(new BattleEvent(BattleEventType.StartedEnemyTurn)));
            yield return StartCoroutine(StartChainOfEvents(new BattleEvent(BattleEventType.StartedEnemyMovementPeriod)));
            yield return StartCoroutine(StartChainOfEvents(new BattleEvent(BattleEventType.EndedEnemyTurn)));
            Debug.Log("------ Ending Enemy Turn ------");
            yield return StartCoroutine(NextPlayerTurn());
        }

        private IEnumerator EndBattle()
        {
            Debug.Log("------ Ending Battle ------");
            yield return StartCoroutine(StartChainOfEvents(new BattleEvent(BattleEventType.EndedBattle)));
            GameState = GameState.Rewards;
            yield return new WaitForSecondsRealtime(ActionExecutionSpeed / 2f);
            CreateEndOfBattlePopup();
        }

        private IEnumerator StartChainOfEvents(BattleEvent battleEvent)
        {
            ExecuteAudioVisualCues(new BattleEventPackage(battleEvent));

            var sequenceOfResponses = Tick(battleEvent);
            while (sequenceOfResponses.MoveNext()) 
            {
                yield return sequenceOfResponses.Current;
            }
        }

        private void ExecuteAudioVisualCues(BattleEventPackage battleEventPackage)
        {
            _battleRenderer.RenderEventPackage(battleEventPackage);
            _sfxManager.TriggerAudio(battleEventPackage);
        }
        
        private IEnumerator Tick(BattleEvent previousBattleEvent)
        {
            if (previousBattleEvent.type == BattleEventType.PlayerDied)
            {
                GameState = GameState.PlayerDied;
                _playerDeathPopup.gameObject.SetActive(true);
                StopAllCoroutines();
            }
            
            while (true)
            {
                var responsePackage = BattleEventsManager.Current.GetNextResponse(previousBattleEvent);
                if (responsePackage.IsEmpty) break;
                ExecuteAudioVisualCues(responsePackage);

                foreach (var battleEvent in responsePackage.battleEvents)
                {
                    // Skip events that are only used for rendering
                    if (battleEvent.type == BattleEventType.EtchingActivated && !battleEvent.showResponse)
                        continue;
                    
                    var waitTime = GetAnimationTime(battleEvent);
                    if (waitTime > 0f)
                        yield return new WaitForSecondsRealtime(waitTime * ActionExecutionSpeed);
                    
                    var sequenceOfResponses = Tick(battleEvent);
                    while (sequenceOfResponses.MoveNext()) 
                    {
                        yield return sequenceOfResponses.Current;
                    }
                    
                    Debug.Log("Finished " + battleEvent.type + ". Back to " + previousBattleEvent.type);
                }
            }
        }

        private float GetAnimationTime(BattleEvent battleEvent)
        {
            var type = battleEvent.type;
            
            switch (type)
            {
                case BattleEventType.PlayerLostLife:
                    return 1.3f;
                case BattleEventType.EtchingActivated when battleEvent.showResponse:
                    return 0.7f;
                case BattleEventType.ItemActivated when battleEvent.showResponse:
                case BattleEventType.EnemySpawned when battleEvent.showResponse:
                case BattleEventType.EndedEnemyTurn:
                case BattleEventType.StartedIndividualEnemyTurn:
                case BattleEventType.EnemyDamaged:
                case BattleEventType.EtchingStunned:
                case BattleEventType.EnemyHealed:
                    return 1f;
                case BattleEventType.EnemyReachedBoat:
                case BattleEventType.EnemyKilled when battleEvent.source != DamageSource.Boat:
                    return 0.75f;
                case BattleEventType.EnemyMove:
                    return 0.5f;
            }

            return -1f;
        }

        private void CreateEndOfBattlePopup()
        {
            _endOfBattlePopup.gameObject.SetActive(true);
            var disturbance = RunProgress.CurrentDisturbance;
            _endOfBattlePopup.SetReward(disturbance);
            _endOfBattlePopup.SetButtonAction(LeaveBattle);
        
        }

        private void LeaveBattle()
        {
            RunProgress.PlayerStats.Gold = Player.Current.Gold;
            RunProgress.PlayerStats.Health = Player.Current.Health;
            DisturbanceLoader.ExecuteEndOfBattleDisturbanceAction(RunProgress.CurrentDisturbance);
            MainManager.Current.LoadOfferScreen();
            Destroy(gameObject);
        }
    }
}