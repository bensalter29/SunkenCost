using System.Collections;
using System.Collections.Generic;
using MapScreen;
using UnityEngine;
using UnityEngine.Serialization;

public enum DisturbanceType
{
    None,
    GoldRush,
    Heart,
    UpgradeCard,
    SpecificCard
}

[CreateAssetMenu(menuName = "Disturbance")]
public class Disturbance : ScriptableObject
{
    public DisturbanceType disturbanceType;
    public Sprite sprite;
    public string title;
    public string description;
    public int amount;
}