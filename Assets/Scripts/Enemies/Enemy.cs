﻿using System.Collections.Generic;
using System.Linq;
using BattleScreen;
using BattleScreen.BattleBoard;
using BattleScreen.BattleEvents;
using Damage;

namespace Enemies
{
    public abstract class Enemy : BattleEventHandler
    {
        public EnemyStats stats;
        
        public EnemyAsset Asset { get; private set; }
        
        public string Name { get; private set; }
        
        public Stat MaxHealthStat { get; private set; }
        protected int Gold { get; set; } = 1;
        public int Health { get; private set; }
        
        public int TurnOrder { get; private set; }

        public float Size { get; protected set; } = 1;

        public bool IsDestroyed { get; private set; } = false;

        public string Speech { get; private set; } = "";

        public EnemyMover Mover { get; } = new EnemyMover();

        public int MaxHealth => MaxHealthStat.Value;

        public Plank Plank => Board.Current.GetPlank(Mover.PlankNum);
        public int PlankNum => Mover.PlankNum;

        public bool FinishedMoving => Mover.FinishedMoving;

        protected override void Awake()
        {
            base.Awake();
            Asset = EnemyLoader.EnemyAssets.First(a => a.Class == GetType());
            stats = new EnemyStats(ResponderID, Asset.Modifiers);
            Name = Asset.Name;
            MaxHealthStat = new Stat(Asset.MaxHealth);
            Health = MaxHealthStat.Value;
        }

        public void Initialise(int plankNum)
        {
            Mover.Init(Asset.Moves, plankNum);
        }
        
        public override List<BattleEventResponseTrigger> GetBattleEventResponseTriggers()
        {
            var responseTriggers = new List<BattleEventResponseTrigger>
            {
                AddResponseTrigger(BattleEventType.EnemyAttacked, DamageOrDie, GetIfThisWasCreatedByMe),
                AddActionTrigger(BattleEventType.EndedEnemyTurn, Mover.EndTurn),
                AddActionTrigger(BattleEventType.PlankMoved, RefreshPlankNum),
                AddActionTrigger(BattleEventType.EtchingsOrderChanged, RefreshPlankNum),
                AddActionTrigger(BattleEventType.PlankCreated, RefreshPlankNum),
                AddActionTrigger(BattleEventType.PlankDestroyed, RefreshPlankNum),
            };
            
            responseTriggers.AddRange(GetEnemyBattleEventResponseTriggers());

            return responseTriggers;
        }

        public virtual int GetBoatDamage()
        {
            return 1;
        }

        public BattleEvent DealPoisonDamage()
        {
            return DamageHandler.DamageEnemy
                (stats.Poison, ResponderID, DamageSource.Poison);
        }

        public BattleEvent Block(int blockAmount)
        {
            Mover.Block(blockAmount);
            return CreateEvent(BattleEventType.EnemyBlocked, blockAmount);
        }
        
        public BattleEvent Heal(int amount)
        {
            var healAmount = amount;
            var healthDifference = (MaxHealthStat.Value - Health);
            if (healthDifference < healAmount) healAmount = healthDifference;
            
            ChangeHealth(healAmount);

            return CreateEvent(BattleEventType.EnemyHealed, healAmount);
        }

        public BattleEvent AddMaxHealthModifier(StatModifier statModifier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealthStat.AddModifier(statModifier);

            var difference = MaxHealth - oldMaxHealth;

            if (difference > 0)
            {
                ChangeHealth(difference);
            }

            if (Health > MaxHealth)
                Health = MaxHealth;
            
            var maxHealthModifiedEvent = CreateEvent(BattleEventType.EnemyMaxHealthModified);
            maxHealthModifiedEvent.modifier = (int)statModifier.Value;
            return maxHealthModifiedEvent;
        }

        public void RemoveMaxHealthModifier(StatModifier statModifier)
        {
            MaxHealthStat.RemoveModifier(statModifier);
            ChangeHealth(0);
        }

        public void SetTurnOrder(int turnOrder)
        {
            TurnOrder = turnOrder;
        }

        public abstract string GetDescription();

        protected virtual List<BattleEventResponseTrigger> GetEnemyBattleEventResponseTriggers()
        {
            return new List<BattleEventResponseTrigger>();
        }

        protected BattleEvent CreateEvent(BattleEventType type, int modifier = 0,
            DamageSource damageSource = DamageSource.None)
        {
            return new BattleEvent(type) 
                {creatorID = ResponderID, modifier = modifier, source = damageSource};
        }
        
        protected BattleEvent Speak(string text)
        {
            Speech = text;
            return CreateEvent(BattleEventType.EnemySpeaking);
        }
        
        private void RefreshPlankNum()
        {
            if (Mover.PlankNum == -1) return;
            Mover.SetPlankNum(transform.parent.GetSiblingIndex());
        }
        
        private void ChangeHealth(int amount)
        {
            Health += amount;
        }
        
        private BattleEventPackage DamageOrDie(BattleEvent previousBattleEvent)
        {
            ChangeHealth(-previousBattleEvent.modifier);
                
            // Killed 
            if (Health <= 0)
            {
                return Die(previousBattleEvent.source);
            }

            var damagedEvent = CreateEvent
                (BattleEventType.EnemyDamaged, previousBattleEvent.modifier, previousBattleEvent.source);
            return new BattleEventPackage(damagedEvent);
        }

        private BattleEventPackage Die(DamageSource source)
        {
            IsDestroyed = true;
            
            var eventList = new List<BattleEvent>();
            
            eventList.Add(CreateEvent(BattleEventType.EnemyKilled, damageSource: source));
            eventList.Add(CreateEvent(BattleEventType.TryGainedGold, Gold, source));
            return new BattleEventPackage(eventList);
        }
    }
}
