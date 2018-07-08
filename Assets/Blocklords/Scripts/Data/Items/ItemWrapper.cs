using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO -> split out the data into components and make this a proper entity
[CreateAssetMenu(fileName = "Item", menuName = "Item Wrapper", order = 1)]
public class ItemWrapper : ScriptableObject
{
    public Item Item;
}
