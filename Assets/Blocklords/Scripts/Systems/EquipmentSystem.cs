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
    [Inject] private StreamSystem StreamSystem { get; set; }

    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Transform unequippedItemParent;
    //[SerializeField] private Transform equippedItemParent;
    [SerializeField] private ItemsTransformsTable equippedItemsParents;

    private IGroup unequippedItemCollections;
    private IGroup equippedItemCollections;

    private Dictionary<Item, GameObject> inventoryItemInstanceTable = new Dictionary<Item, GameObject>();
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

        StreamSystem.ItemEquippedStream.Subscribe(evt =>
        {
            var item = evt.Item;
            if (inventoryItemInstanceTable.ContainsKey(item))
            {
                Destroy(inventoryItemInstanceTable[item]);
                inventoryItemInstanceTable.Remove(item);
            }

            var go = CreateIcon(item, equippedItemsParents[item.ItemType.Value], equippedItemInstanceTable);
            go.OnPointerClickAsObservable().Subscribe(_ =>
            {
                //HACK
                GameDataSystem.SelectedHero.Value.GetComponent<ItemCollectionComponent>().Items.Remove(item);
                GameDataSystem.SelectedPlayerData.Value.GetComponent<ItemCollectionComponent>().Items.Add(item);
            }).AddTo(go);

        }).AddTo(this.Disposer);

        StreamSystem.ItemAddedToInventoryStream.Subscribe(evt =>
        {
            var item = evt.Item;
            if(equippedItemInstanceTable.ContainsKey(item))
            {
                Destroy(equippedItemInstanceTable[item]);
                equippedItemInstanceTable.Remove(item);
            }

            var go = CreateIcon(item, unequippedItemParent, inventoryItemInstanceTable);
            go.OnPointerClickAsObservable().Subscribe(_ =>
            {
                //HACK
                GameDataSystem.SelectedPlayerData.Value.GetComponent<ItemCollectionComponent>().Items.Remove(item);
                GameDataSystem.SelectedHero.Value.GetComponent<ItemCollectionComponent>().Items.Add(item);
            }).AddTo(go);
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
