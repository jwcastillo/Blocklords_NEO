using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using UniRx;
using System;

public class HeroClassStatsSystem : SystemBehaviour
{
    private IGroup heroes;

    [SerializeField] private ClassStatsWrapperTable classStatsWrapperTable;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        heroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(ViewComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        //delay a frame to check whether it's a saved hero or new hero that requires setup
        heroes.OnAdd().DelayFrame(1).Where(e => !e.HasComponent<PlayerDataComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();

            heroComponent.Class.DistinctUntilChanged().Subscribe(heroClass =>
            {
                heroComponent.BaseStats = classStatsWrapperTable[heroClass].Stats;
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);
    }
}

[Serializable]
public class ClassStatsTable : SerializableDictionary<HeroClass, Stats> { }

[Serializable]
public class ClassStatsWrapperTable : SerializableDictionary<HeroClass, StatsWrapper> { }