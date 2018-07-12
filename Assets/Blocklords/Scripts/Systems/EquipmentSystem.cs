using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using System;
using UniRx;
using System.Linq;
using Zenject;
using UniRx.Triggers;

public class EquipmentSystem : SystemBehaviour
{
    //[Inject] private SerializableHeroes SerializableHeroes { get; set; }
    [Inject] private GameDataSystem GameDataSystem { get; set; }

    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Transform unequippedItemParent;
    //[SerializeField] private Transform equippedItemParent;
    [SerializeField] private ItemsTransformsTable equippedItemsParents;

    private IGroup unequippedItemCollections;
    private IGroup equippedItemCollections;

    private Dictionary<Item, GameObject> unequippedItemInstanceTable = new Dictionary<Item, GameObject>();
    private Dictionary<Item, GameObject> equippedItemInstanceTable = new Dictionary<Item, GameObject>();

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        Func<IEntity, ReactiveProperty<bool>> doesntHaveHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => !e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        unequippedItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, doesntHaveHeroComponent );

        Func<IEntity, ReactiveProperty<bool>> hasHeroComponent = (e) =>
        {
            return e.Components.ObserveEveryValueChanged(_ => e.HasComponent<HeroComponent>()).ToReactiveProperty();
        };
        equippedItemCollections = this.CreateGroup(new HashSet<Type>() { typeof(ItemCollectionComponent) }, hasHeroComponent);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        unequippedItemCollections.OnAdd().Subscribe(entity =>
        {
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();

            itemCollectionComponent.Items.ObserveEveryValueChanged(c => c.Count).Subscribe(c =>
            {
                var removedItems = unequippedItemInstanceTable.Keys.Where(key => !itemCollectionComponent.Items.Contains(key)).ToList();
                var addedItems = itemCollectionComponent.Items.Where(item => !unequippedItemInstanceTable.ContainsKey(item)).ToList();

                foreach(var item in removedItems)
                {
                    Destroy(unequippedItemInstanceTable[item]);
                    unequippedItemInstanceTable.Remove(item);
                }

                foreach (var item in addedItems)
                {
                    var go = CreateIcon(item, unequippedItemParent, unequippedItemInstanceTable);
                    //TODO -> split out into separate group.OnAdd()?
                    go.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        //HACK
                        var targetCollection = GameDataSystem.SelectedHero.Value.GetComponent<ItemCollectionComponent>();
                        targetCollection.Items.Add(item);
                        itemCollectionComponent.Items.Remove(item);
                    }).AddTo(go);
                }
            }).AddTo(this.Disposer).AddTo(itemCollectionComponent.Disposer);
        }).AddTo(this.Disposer);

        //unequippedItemCollections.OnRemove().Subscribe(entity =>
        //{
        //    foreach(var kvp in unequippedItemInstanceTable)
        //    {
        //        Debug.Log(kvp.Key == null);
        //    }
        //}).AddTo(this.Disposer);

        equippedItemCollections.OnAdd().Subscribe(entity =>
        {
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();

            itemCollectionComponent.Items.ObserveEveryValueChanged(c => c.Count).Subscribe(c =>
            {
                var removedItems = equippedItemInstanceTable.Keys.Where(key => !itemCollectionComponent.Items.Contains(key)).ToList();
                var addedItems = itemCollectionComponent.Items.Where(item => !equippedItemInstanceTable.ContainsKey(item)).ToList();

                foreach (var item in removedItems)
                {
                    Destroy(equippedItemInstanceTable[item]);
                    equippedItemInstanceTable.Remove(item);
                }

                foreach (var item in addedItems)
                {
                    var go = CreateIcon(item, equippedItemsParents[item.ItemType.Value], equippedItemInstanceTable);
                    go.OnPointerClickAsObservable().Subscribe(_ =>
                    {
                        var targetCollection = GameDataSystem.SelectedPlayerData.Value.GetComponent<ItemCollectionComponent>();
                        targetCollection.Items.Add(item);
                        itemCollectionComponent.Items.Remove(item);
                    }).AddTo(go);
                }
            }).AddTo(this.Disposer).AddTo(itemCollectionComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    private GameObject CreateIcon(Item item, Transform parent, Dictionary<Item, GameObject> itemsInstanceTable)
    {
        var itemIconEntity = PoolManager.GetPool().CreateEntity();
        var go = PrefabFactory.Instantiate(itemIconEntity, itemPrefab, parent);
        var itemIconComponent = itemIconEntity.GetComponent<ItemIconComponent>();
        itemIconComponent.Item.Value = item;
        itemsInstanceTable.Add(item, go);

        return go;
    }
}

[Serializable]
public class ItemsTransformsTable : SerializableDictionary<ItemType, Transform> { }
