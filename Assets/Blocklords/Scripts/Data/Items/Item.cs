using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System;

//TODO -> split out the data into components and make this a proper entity
[Serializable]
public class Item
{
    public StringReactiveProperty ID = new StringReactiveProperty();
    public StringReactiveProperty Name = new StringReactiveProperty();

    public StringReactiveProperty Description = new StringReactiveProperty();

    public Stats BaseStats = new Stats();
    public Stats MaxStats = new Stats();

    public ItemTypeReactiveProperty ItemType;
    public ItemQualityReactiveProperty ItemQuality;

    //public BoolReactiveProperty IsEquipped = new BoolReactiveProperty();
    //public EntityReactiveProperty TargetHero = new EntityReactiveProperty();

    public Item Clone()
    {
        return (Item)this.MemberwiseClone();
    }
}

[Serializable]
public class ItemReactiveProperty : ReactiveProperty<Item>
{
    public ItemReactiveProperty() { }
    public ItemReactiveProperty(Item initialValue) : base(initialValue) { }
}

public enum ItemQuality
{
    Common = 0,
    Rare = 1,
    Legendary = 2,
}

[Serializable]
public class ItemQualityReactiveProperty : ReactiveProperty<ItemQuality>
{
    public ItemQualityReactiveProperty() { }
    public ItemQualityReactiveProperty(ItemQuality initialValue) : base(initialValue) { }
}

public enum ItemType
{
    Head = 0,
    Body = 1,
    Hands = 2,
    Shield = 3,
    Weapon = 4,
}

[Serializable]
public class ItemTypeReactiveProperty : ReactiveProperty<ItemType>
{
    public ItemTypeReactiveProperty() { }
    public ItemTypeReactiveProperty(ItemType initialValue) : base(initialValue) { }
}
