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
    public IObservable<ItemCollectionChangedEvent> ItemEquippedStream
    {
        get
        {
            return heroItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();
                var itemStream = itemCollectionComponent.Items.OnAdd().Select(evt =>
                {
                    var itemEquippedEvent = new ItemCollectionChangedEvent(evt.Value, e);
                    return itemEquippedEvent;
                });

                return itemStream;
            });
        }
    }

    public IObservable<ItemCollectionChangedEvent> ItemUnequippedStream
    {
        get
        {
            return heroItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();
                var itemStream = itemCollectionComponent.Items.OnRemove().Select(evt =>
                {
                    var itemEquippedEvent = new ItemCollectionChangedEvent(evt.Value, e);
                    return itemEquippedEvent;
                });

                return itemStream;
            });
        }
    }

    public IObservable<ItemCollectionChangedEvent> ItemAddedToInventoryStream
    {
        get
        {
            return inventoryItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();
                var itemStream = itemCollectionComponent.Items.OnAdd().Select(evt =>
                {
                    var itemEquippedEvent = new ItemCollectionChangedEvent(evt.Value, e);
                    return itemEquippedEvent;
                });

                return itemStream;
            });
        }
    }

    public IObservable<ItemCollectionChangedEvent> ItemRemovedFromInventoryStream
    {
        get
        {
            return inventoryItemCollections.OnAdd().SelectMany(e =>
            {
                var itemCollectionComponent = e.GetComponent<ItemCollectionComponent>();
                var itemStream = itemCollectionComponent.Items.OnRemove().Select(evt =>
                {
                    var itemEquippedEvent = new ItemCollectionChangedEvent(evt.Value, e);
                    return itemEquippedEvent;
                });

                return itemStream;
            });
        }
    }

    private IGroup inventoryItemCollections;
    private IGroup heroItemCollections;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        Func<IEntity, ReactiveProperty<bool>> doesntHaveHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => !e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        inventoryItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, doesntHaveHeroComponent);

        Func<IEntity, ReactiveProperty<bool>> hasHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        heroItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, hasHeroComponent);
    }
}
