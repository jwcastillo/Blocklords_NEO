using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using SubjectNerd.Utilities;
using UniRx;

public class ItemCollectionComponent : ComponentBehaviour
{
    [Reorderable] public List<Item> Items = new List<Item>();
    //public ReactiveCollection<ItemBase> Items = new ReactiveCollection<ItemBase>();
}
