using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;
using System;
using SubjectNerd.Utilities;

public class HeroComponent : ComponentBehaviour
{
    public StringReactiveProperty ID = new StringReactiveProperty();
    public StringReactiveProperty Name = new StringReactiveProperty();

    public HeroClassReactiveProperty Class;

    public Stats BaseStats;

    [Reorderable] public List<Stats> ModifierStats = new List<Stats>();
    //public List<Equipment> Equipment = new List<Equipment>();

    //public IntReactiveProperty Leadership = new IntReactiveProperty();
    //public IntReactiveProperty Intelligence = new IntReactiveProperty();
    //public IntReactiveProperty Strength = new IntReactiveProperty();
    //public IntReactiveProperty Defense = new IntReactiveProperty();
    //public IntReactiveProperty Speed = new IntReactiveProperty();
}

[Serializable]
public class HeroComponentReactiveProperty : ReactiveProperty<HeroComponent>
{
    public HeroComponentReactiveProperty() { }
    public HeroComponentReactiveProperty(HeroComponent initialValue) : base(initialValue) { }
}

public enum HeroClass
{
    Archer = 0,
    Ranger = 1,
    Soldier = 2,
}

[Serializable]
public class HeroClassReactiveProperty : ReactiveProperty<HeroClass>
{
    public HeroClassReactiveProperty() { }
    public HeroClassReactiveProperty(HeroClass initialValue) : base(initialValue) { }
}
