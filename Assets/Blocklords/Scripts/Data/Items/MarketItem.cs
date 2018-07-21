using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System;

//TODO -> split out the data into components and make this a proper entity
[Serializable]
public class MarketItem
{
    public ItemReactiveProperty item = new ItemReactiveProperty();
    public DecimalReactiveProperty price = new DecimalReactiveProperty();

    public ByteArrayReactiveProperty ownerAddress = new ByteArrayReactiveProperty();

    public MarketItem(Item item, decimal price, byte[] owner)
    {
        this.item.Value = item;
        this.price.Value = price;
        this.ownerAddress.Value = owner;
    }
    public void SetPrice(decimal price)
    {
        if (this.price.Value != price)
        {
            this.price.Value = price;
        }
    }

    public static bool IsNull(MarketItem marketItem)
    {
        return (marketItem == null);
    }

    public static string GetItemId(MarketItem marketItem)
    {
        if (MarketItem.IsNull(marketItem)) return "";

        return marketItem.item.Value.ID.Value;
    }
}
/*
[Serializable]
public class ItemReactiveProperty : ReactiveProperty<Item>
{
    public ItemReactiveProperty() { }
    public ItemReactiveProperty(Item initialValue) : base(initialValue) { }
}

    */

[Serializable]
public class DecimalReactiveProperty : ReactiveProperty<Decimal>
{
    public DecimalReactiveProperty() { }
    public DecimalReactiveProperty(decimal initialValue) : base(initialValue) { }
}

[Serializable]
public class ByteArrayReactiveProperty : ReactiveProperty<byte[]>
{
    public ByteArrayReactiveProperty() { }
    public ByteArrayReactiveProperty(byte[] initialValue) : base(initialValue) { }
}