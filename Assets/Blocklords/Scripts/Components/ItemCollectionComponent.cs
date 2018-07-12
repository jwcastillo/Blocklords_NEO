using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using SubjectNerd.Utilities;
using UniRx;

public class ItemCollectionComponent : ComponentBehaviour
{
    [Reorderable] public List<Item> Items = new List<Item>();
    //[SerializeField] [Reorderable] private List<Item> items = new List<Item>();

    //public ItemCollection ItemsRx = new ItemCollection();
    //public ReactiveCollection<Item> Items = new ReactiveCollection<Item>();

    //private void Awake()
    //{
    //    items.ObserveEveryValueChanged(i => i.Count).Subscribe(_ =>
    //    {
    //        foreach(var item in items)
    //        {
                
    //        }
    //    }).AddTo(this.Disposer);
    //}
}

//[System.Serializable]
//public class ItemCollection : ReactiveCollection<Item> { }
