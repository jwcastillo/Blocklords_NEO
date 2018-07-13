using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System;
using TMPro;
using System.Linq;

[Serializable]
public class Stats
{
    public IntReactiveProperty Leadership = new IntReactiveProperty();
    public IntReactiveProperty Intelligence = new IntReactiveProperty();
    public IntReactiveProperty Strength = new IntReactiveProperty();
    public IntReactiveProperty Defense = new IntReactiveProperty();
    public IntReactiveProperty Speed = new IntReactiveProperty();
}

//TODO -> move to ECS
[Serializable]
public class StatsText
{
    public TextMeshProUGUI LeadershipText;
    public TextMeshProUGUI IntelligenceText;
    public TextMeshProUGUI StrengthText;
    public TextMeshProUGUI DefenseText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI TroopsText;
}

public static class StatsTextExtensions
{
    public static void Update(this StatsText statsText, Stats baseStats, IEnumerable<Stats> modifierStats)
    {
        statsText.LeadershipText.text = (baseStats.Leadership.Value + modifierStats.Select(stat => stat.Leadership.Value).Sum()).ToString();
        statsText.IntelligenceText.text = (baseStats.Intelligence.Value + modifierStats.Select(stat => stat.Intelligence.Value).Sum()).ToString();
        statsText.StrengthText.text = (baseStats.Strength.Value + modifierStats.Select(stat => stat.Strength.Value).Sum()).ToString();
        statsText.DefenseText.text = (baseStats.Defense.Value + modifierStats.Select(stat => stat.Defense.Value).Sum()).ToString();
        statsText.SpeedText.text = (baseStats.Speed.Value + modifierStats.Select(stat => stat.Speed.Value).Sum()).ToString();
        //statsText.troopsText.text = baseStats.Troops.Value.ToString(); //TODO -> this is a computed value
    }  
}
