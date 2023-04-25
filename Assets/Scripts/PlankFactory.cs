﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleScreen;
using BattleScreen.BattleBoard;
using BattleScreen.BattleEvents;
using BattleScreen.Events;
using Etchings;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlankFactory : MonoBehaviour
{
    public static PlankFactory Current;
    
    [SerializeField] private GameObject _plankPrefab;
    [SerializeField] private Transform _plankGrid;
    
    private Board _board;

    private void Awake()
    {
        if (Current)
            Destroy(Current.gameObject);

        Current = this;
    }

    public void Start()
    {
        _board = _plankGrid.parent.GetComponent<Board>();
    }
    

    public Plank CreatePlank()
    {
        var newPlank = Instantiate(_plankPrefab, _plankGrid);
        newPlank.transform.SetSiblingIndex(_plankGrid.childCount);
        _board.Refresh();

        return newPlank.GetComponent<Plank>();
    }

    public List<BattleEvent> DestroyPlank(DamageSource source, int plankPosition = -1, Plank plank = null)
    {
        if (plankPosition != -1)
            plank = _board.GetPlank(plankPosition);

        if (plank is null)
        {
            throw new Exception("Trying to destroy plank that isn't there!");
        }
        
        return plank.Destroy(source);
    }
    
    private IEnumerator WaitForPlankDestruction(PlankDisplay plankDisplay)
    {
        yield return 0;
        _board.Refresh();
    }
}