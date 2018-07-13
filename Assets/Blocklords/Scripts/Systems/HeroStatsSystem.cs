using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using UniRx;
using System;
using Zenject;

public class HeroStatsSystem : SystemBehaviour
{
    [Inject] private StreamSystem StreamSystem { get; set; }
    [Inject] private GameDataSystem GameDataSystem { get; set; }

    [Inject] private SerializableHeroes SerializableHeroes { get; set; }
    [Inject] private NonSerializableHeroes NonSerializableHeroes { get; set; }

    [SerializeField] private ClassStatsWrapperTable classStatsWrapperTable;

    public override void OnEnable()
    {
        base.OnEnable();

        NonSerializableHeroes.OnAdd().Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();

            heroComponent.Class.DistinctUntilChanged().Subscribe(heroClass =>
            {
                heroComponent.BaseStats = classStatsWrapperTable[heroClass].Stats;
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);

        StreamSystem.ItemEquippedStream.Subscribe(item =>
        {
            var entity = GameDataSystem.SelectedHero.Value;
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
            var heroComponent = entity.GetComponent<HeroComponent>();

            if (!heroComponent.ModifierStats.Contains(item.BaseStats))
            {
                heroComponent.ModifierStats.Add(item.BaseStats);
            }
        }).AddTo(this.Disposer);

        StreamSystem.ItemUnequippedStream.Subscribe(item =>
        {
            foreach (var entity in SerializableHeroes.Entities)
            {
                var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
                var heroComponent = entity.GetComponent<HeroComponent>();

                if (heroComponent.ModifierStats.Contains(item.BaseStats))
                {
                    heroComponent.ModifierStats.Remove(item.BaseStats);
                }
            }
        }).AddTo(this.Disposer);
    }
}

[Serializable]
public class ClassStatsTable : SerializableDictionary<HeroClass, Stats> { }

[Serializable]
public class ClassStatsWrapperTable : SerializableDictionary<HeroClass, StatsWrapper> { }