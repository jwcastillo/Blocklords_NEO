using AlphaECS;
using System;
using UniRx;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Heroes : Group
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
    {
        Components = new HashSet<Type> { typeof(HeroComponent), };

        base.Initialize(eventSystem, poolManager);
    }
}

public class SerializableHeroes : Group
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
    {
        Components = new HashSet<Type> { typeof(HeroComponent), typeof(PlayerDataComponent), typeof(ItemCollectionComponent), };

        base.Initialize(eventSystem, poolManager);
    }
}

public class NonSerializableHeroes : Group
{
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
    {
        Components = new HashSet<Type> { typeof(HeroComponent), };

        Func<IEntity, ReactiveProperty<bool>> hasPlayerDataComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => !e.HasComponent<PlayerDataComponent>()).ToReactiveProperty();
        };

        Predicates.Add(hasPlayerDataComponent);

        base.Initialize(eventSystem, poolManager);
    }
}