using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;
using System;
using SubjectNerd.Utilities;
using System.Linq;

public class HeroComponent : ComponentBehaviour
{
    public StringReactiveProperty ID = new StringReactiveProperty();
    public StringReactiveProperty Name = new StringReactiveProperty();

    public HeroClassReactiveProperty Class;

    public Stats BaseStats;

//    [HideInInspector] public ReactiveCollection<Stats> ModifierStats = new ReactiveCollection<Stats>();
    [Reorderable] public List<Stats> ModifierStats = new List<Stats>();

//#if UNITY_EDITOR
//    [SerializeField] [Reorderable] private List<Stats> modifierStats = new List<Stats>();

//    //NOTE -> want to use awake here but for some reason the inspector does not refresh correctly when doing this on startup
//    //private void Awake()
//    private void Start()
//    {
//        ModifierStats.ObserveAdd().Select(_ => true).Merge(ModifierStats.ObserveRemove().Select(_ => true)).StartWith(true).Subscribe(_ =>
//        {
//            modifierStats.Clear();
//            foreach(var stat in ModifierStats)
//            {
//                modifierStats.Add(stat);
//            }
//        }).AddTo(this.Disposer);
//    }
//#endif
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
