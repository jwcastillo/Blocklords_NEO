using System.Collections;
using System.Collections.Generic;
using AlphaECS;
using UnityEngine;

public class ItemCollectionChangedEvent
{
    public Item Item { get; set; }
    public IEntity Collection { get; set; }

    public ItemCollectionChangedEvent(Item item, IEntity collection)
    {
        Item = item;
        Collection = collection;
    }
}

