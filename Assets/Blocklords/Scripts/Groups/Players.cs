using AlphaECS;
using System;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Players : Group
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
    {
        Components = new HashSet<Type> { typeof(PlayerDataComponent), };

        base.Initialize(eventSystem, poolManager);
    }
}

//TODO -> have this check that it's ONLY player data, not just against HeroComponents?
public class CorePlayers : Group
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
    {
        Components = new HashSet<Type> { typeof(PlayerDataComponent), typeof(ItemCollectionComponent), };

        Func<IEntity, ReactiveProperty<bool>> hasHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => !e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };

        Predicates.Add(hasHeroComponent);

        base.Initialize(eventSystem, poolManager);
    }
}