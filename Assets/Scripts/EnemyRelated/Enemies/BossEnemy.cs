﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    // Smashes the last stick every X turns
    private static int abilityCooldown = 3;
    [SerializeField] private int cooldownCounter = 0;

    protected override void Awake()
    {
        Size = 1.2f;
        Name = "Cucunger";
        MoveMax = 1;
        MoveMin = 1;
        MaxHealth = 60;
        Gold = 5;

        base.Awake();
    }

    protected override void Start()
    {
        GameEvents.current.BossSpawned();
        
        base.Start();
    }

    public override string GetDescription()
    {
        var turns = abilityCooldown - cooldownCounter;

        var turnText = "";
        switch (turns)
        {
            case 0:
                turnText = "this turn";
                break;
            case 1:
                turnText = "in 1 turn";
                break;
            default:
                turnText = "in " + turns + " turns";
                break;
        }
        
        return "Destroys the furthest plank to the right " + turnText;
    }

    protected override void PreMovingAbility()
    {
        var speakText = (abilityCooldown - cooldownCounter).ToString();
        if (speakText == "0")
            speakText = "X";

        Speak(speakText);
        
        if (cooldownCounter >= abilityCooldown && StickNum != StickManager.current.stickCount-1)
        {
            cooldownCounter = 0;
            StickManager.current.DestroyStick(StickManager.current.stickCount-1);
        }
        else
        {
            cooldownCounter++;
        }
        
        base.PreMovingAbility();
    }

    protected override bool TestForPreMovingAbility()
    {
        return true;
    }

    public override void DestroySelf(bool killedByPlayer)
    {
        GameEvents.current.BossKilled();
        base.DestroySelf(killedByPlayer);
    }
}
