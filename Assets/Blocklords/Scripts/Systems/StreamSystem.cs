using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;
using AlphaECS;
using System;
using System.Linq;

public class StreamSystem : SystemBehaviour
{
    public IObservable<Item> ItemEquippedStream
    {
        get
        {
            return equippedItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();

                var previousItems = new List<Item>();
                var itemStream = itemCollectionComponent.Items.ObserveEveryValueChanged(c => c.Count)
                    //.Pairwise()
                    //.StartWith(new Pair<int>(0, 0))
                    //.Where(pair => pair.Current > pair.Previous)
                .SelectMany(pair =>
                {
                    var addedItems = itemCollectionComponent.Items.Where(item => !previousItems.Contains(item)).ToList();
                    previousItems = itemCollectionComponent.Items.ToList();
                    return addedItems;
                });

                return itemStream;
            });
        }
    }

    public IObservable<Item> ItemUnequippedStream
    {
        get
        {
            return unequippedItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();

                var previousItems = new List<Item>();
                var itemStream = itemCollectionComponent.Items.ObserveEveryValueChanged(c => c.Count)
                    //.Pairwise()
                    //.StartWith(new Pair<int>(0, 0))
                    //.Where(pair => pair.Current > pair.Previous)
                .SelectMany(pair =>
                {
                    var addedItems = itemCollectionComponent.Items.Where(item => !previousItems.Contains(item)).ToList();
                    previousItems = itemCollectionComponent.Items.ToList();
                    return addedItems;
                });

                return itemStream;
            });
        }
    }

    private IGroup unequippedItemCollections;
    private IGroup equippedItemCollections;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        Func<IEntity, ReactiveProperty<bool>> doesntHaveHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => !e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        unequippedItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, doesntHaveHeroComponent);

        Func<IEntity, ReactiveProperty<bool>> hasHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        equippedItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, hasHeroComponent);
    }
}
