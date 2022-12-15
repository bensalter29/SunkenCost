﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxolitlEnemy : Enemy
{
    private int _healingAmount = 5;
    
    protected override void Awake()
    {
        Name = "Axolitl";
        MoveSet.Add(1);
        MoveSet.Add(0);
        MoveSet.Add(2);
        MaxHealth = 15;
        Gold = 1;

        base.Awake();
    }
    
    public override string GetDescription()
    {
        return "Gains health when it doesn't move";
    }

    protected override void PreMovingAbility()
    {
        // Heal 2 up to max heath, add to max health if not
        if (Health < MaxHealth)
        {
            if (MaxHealth - _healingAmount > _healingAmount)
            {
                Heal(_healingAmount);
            }
            else
            {
                Heal(MaxHealth);
            }
        }
        else
        {
            MaxHealth += _healingAmount;
            Heal(_healingAmount);
        }
        base.PreMovingAbility();
    }

    protected override bool TestForPreMovingAbility()
    {
        // Only if not moving
        return NextMove == 0;
    }
}